using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath;

public class Link : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly LinkInfo _linkInfo;
    private readonly ILogger? _logger;
    private readonly Channel<dynamic> _rxqu;

    public LinkInfo LinkInfo => _linkInfo;
    public string Host => _host;
    public int Port => _port;
    
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isFini = false;
    private TcpClient _tcpClient;
    private Stream _stream;
    
    public event EventHandler? OnFini;

    private Link(string host, int port, LinkInfo linkInfo, ILogger? logger = null)
    {
        _host = host;
        _port = port;
        _linkInfo = linkInfo;
        _logger = logger;
        _rxqu = Channel.CreateUnbounded<dynamic>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool IsFini => _isFini;

    public static async Task<Link> Connect(string host, int port, LinkInfo linkInfo, ILogger? logger = null)
    {
        logger?.LogTrace("Request a new link connection {Host}:{Port}", host, port);
        var link = new Link(host, port, linkInfo, logger);
        logger?.LogTrace("New link instance created: {LinkId}", link.GetHashCode().ToString("X4"));
        if (await link.ConnectAsync())
            logger?.LogTrace("Link {LinkId} connected", link.GetHashCode().ToString("X4"));
        else
            logger?.LogTrace("Link {LinkId} failed to connected", link.GetHashCode().ToString("X4"));
        return link;
    }

    private async Task<bool> ConnectAsync()
    {
        try
        {
            _logger?.LogTrace("Start to connect the link {LinkId}", this.GetHashCode().ToString("X4"));
            _tcpClient = new TcpClient(_host, _port);
            _logger?.LogTrace("Tcp client connected on link {LinkId}", this.GetHashCode().ToString("X4"));
        }
        catch (SocketException socketException)
        {
            _logger?.LogWarning("Tcp client failed for link {LinkId} to connect: {ExceptionMessage}", 
                this.GetHashCode().ToString("X4"),
                socketException.Message);
            Finish();
            return false;
        }

        if (LinkInfo?.clientCertificates == null)
        {
            _logger?.LogTrace("Getting the stream for link {LinkId}", this.GetHashCode().ToString("X4"));
            _stream = _tcpClient.GetStream();   
        }
        else
        {
            // _stream = _tcpClient.GetStream();
            _logger?.LogTrace("Will create the SSL stream for link {LinkId}", this.GetHashCode().ToString("X4"));
        
            var _sslStream = new SslStream(
                _tcpClient.GetStream(),
                true,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null);

            _logger?.LogTrace("Will authenticate");
        
            // The server name must match the name on the server certificate.
            try
            {
                _logger?.LogTrace("Will authenticate as a client on {Host} for link {LinkId}", _host, this.GetHashCode().ToString("X4"));
                await _sslStream.AuthenticateAsClientAsync(_host, LinkInfo.clientCertificates, SslProtocols.None, false);
            
                _logger?.LogTrace("Link {LinkId} authenticated using SSL certificates", this.GetHashCode().ToString("X4"));
            }
            catch (AuthenticationException e)
            {
                _logger?.LogError("Exception on link {LinkId}: {ErrorMessage}", this.GetHashCode().ToString("X4"), e.Message);
                if (e.InnerException != null)
                {
                    _logger?.LogError("Inner exception: {0}", e.InnerException.Message);
                }
                _logger?.LogError("Authentication failed on link {LinkId} closing the connection", this.GetHashCode().ToString("X4"));
                _tcpClient.Close();
            
                return false;
            }
            _stream = _sslStream;
        }
        
        if (_stream != null)
        {
            _logger?.LogTrace("Socket is created for {LinkId}, will create the pipes", this.GetHashCode().ToString("X4"));
            var pipe = new Pipe();
            Task writing = FillPipeAsync(pipe.Writer, _cancellationTokenSource.Token);
            Task reading = ReadPipeAsync(pipe.Reader, _cancellationTokenSource.Token);
            return true;
        }

        return false;
    }

    public bool ValidateServerCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // TODO Check server certificate
        return true;
    }

    public async Task Tx<T>(TelepathMessage<T> telepathMessage)
    {
        _logger?.LogTrace($"Called Tx<T>");
        if (_isFini)
        {
            _logger?.LogTrace("Tx isFini on link {LinkId}, throwing exception", this.GetHashCode().ToString("X4"));
            throw new SynsharpError();
        }

        byte[] bytes = MessagePackSerializer.Serialize(telepathMessage);
        _logger?.LogTrace(BitConverter.ToString(bytes).Replace("-", " "));

        _logger?.LogTrace("Sending message ({BytesLength} bytes) on link {LinkId}", bytes.Length, this.GetHashCode().ToString("X4"));
        await _stream.WriteAsync(bytes, _cancellationTokenSource.Token);
        _logger?.LogTrace("Sent {BytesLength} bytes on link {LinkId}", bytes.Length, this.GetHashCode().ToString("X4"));
    }

    public async Task<dynamic> Rx()
    {
        _logger?.LogTrace("Called Rx on link {LinkId}", this.GetHashCode().ToString("X4"));
        if (_isFini)
        {
            _logger?.LogTrace("Rx isFini on link {LinkId}, throwing exception", this.GetHashCode().ToString("X4"));
            throw new SynsharpError();
        }

        return await _rxqu.Reader.ReadAsync(_cancellationTokenSource.Token);
    }
    
    async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken)
    {
        while (!_isFini)
        {
            try 
            {
                _logger?.LogTrace("Starting pipeline reading loop on link {LinkId}", this.GetHashCode().ToString("X4"));
                using (var stream = reader.AsStream())
                using (var streamReader = new MessagePackStreamReader(stream))
                {
                    ReadOnlySequence<byte>? readAsync;
                    while ((readAsync = await streamReader.ReadAsync(cancellationToken)) != null && !_isFini)
                    {
                        var msgBlock = (ReadOnlySequence<byte>)readAsync;
                        _logger?.LogTrace("Pushing a new message to the queue (len: {MsgBlockLength}) on link {LinkId}", msgBlock.Length, this.GetHashCode().ToString("X4"));
                        _logger?.LogTrace(BitConverter.ToString(msgBlock.ToArray()).Replace("-", " "));
                        await _rxqu.Writer.WriteAsync(MessagePackSerializer.Deserialize<dynamic>(msgBlock), cancellationToken);
                    }
                }   
            }
            catch (OperationCanceledException ex)
            {
                Finish();
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in ReadPipeAsync: {ErrorMessage} on the link {LinkId}", ex.Message, this.GetHashCode().ToString("X4"));
                Finish();
                break;
            }
        }

        _logger?.LogTrace("Stopping pipeline reading loop on link {LinkId}", this.GetHashCode().ToString("X4"));
        await reader.CompleteAsync();
    }
    
    async Task FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
    {
        const int minimumBufferSize = 512;

        while (!_isFini)
        {
            _logger?.LogTrace("Starting pipeline filling loop on link {LinkId}", this.GetHashCode().ToString("X4"));
            // Allocate at least 512 bytes from the PipeWriter
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);
            try
            {
                int bytesRead = await _stream.ReadAsync(memory, cancellationToken);
                // int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                if (bytesRead == 0)
                {
                    _logger?.LogTrace("Read 0 bytes on link {LinkId}, done. ", this.GetHashCode().ToString("X4"));
                    Finish();
                    break;
                }

                _logger?.LogTrace("Read {BytesRead} bytes on link {LinkId}", bytesRead, this.GetHashCode().ToString("X4"));
                // Tell the PipeWriter how much was read from the Socket
                writer.Advance(bytesRead);
            }
            catch (OperationCanceledException ex)
            {
                Finish();
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in FillPipeAsync {ErrorMessage} on the link {LinkId}", ex.Message, this.GetHashCode().ToString("X4"));
                Finish();
                break;
            }

            _logger?.LogTrace("Flush link {LinkId}", this.GetHashCode().ToString("X4"));
            // Make the data available to the PipeReader
            FlushResult result = await writer.FlushAsync(cancellationToken);

            if (result.IsCompleted)
            {
                _logger?.LogTrace("Complete, breaking link {LinkId}", this.GetHashCode().ToString("X4"));
                break;
            }
        }

        _logger?.LogTrace("Stopping pipeline filling loop on link {LinkId}", this.GetHashCode().ToString("X4"));
        await writer.CompleteAsync();
    }

    public void Finish()
    {
        if (_isFini)
        {
            _logger?.LogTrace("Link {LinkId} is already finished", this.GetHashCode().ToString("X4"));
            return;
        }

        _isFini = true;
        
        _logger?.LogTrace("Closing link {LinkId}", this.GetHashCode().ToString("X4"));
        _cancellationTokenSource.Cancel();
        if (_tcpClient != null)
        {
            _logger?.LogTrace("Disposing tcpClient on link {LinkId}", this.GetHashCode().ToString("X4"));
            _tcpClient.Close();
            _tcpClient.Dispose();
        }

        if (OnFini != null)
        {
            _logger?.LogTrace("Calling OnFini for the link {LinkId}", this.GetHashCode().ToString("X4"));
            OnFini?.Invoke(this, EventArgs.Empty);   
        }
    }

    public void Dispose()
    {
        Finish();
        _logger?.LogTrace("Disposing link {LinkId}", this.GetHashCode().ToString("X4"));
    }
}