using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath;

internal class Link : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly ILogger? _logger;
    private Socket? _socket;
    private readonly Channel<dynamic> _rxqu;
    
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isFini = false;
    public event EventHandler? OnFini;

    private Link(string host, int port, ILogger? logger = null)
    {
        _host = host;
        _port = port;
        _logger = logger;
        _rxqu = Channel.CreateUnbounded<dynamic>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool IsFini => _isFini;

    public static async Task<Link> Connect(string host, int port, ILogger? logger = null)
    {
        var link = new Link(host, port, logger);
        await link.ConnectAsync();
        return link;
    }

    private async Task ConnectAsync()
    {
        _logger?.LogTrace("Start to connect the link");
        var ipHostInfo = await Dns.GetHostEntryAsync(_host);
        var ipAddress = ipHostInfo.AddressList[0];
        var ipEndPoint = new IPEndPoint(ipAddress, _port);
        
        _socket = new Socket(
            ipEndPoint.AddressFamily, 
            SocketType.Stream,
            ProtocolType.Tcp);
        
        await _socket.ConnectAsync(ipEndPoint);
        _logger?.LogTrace("Socket is created");
        
        _logger?.LogDebug("Create the pipeline to read data from socket.");
        var pipe = new Pipe();
        Task writing = FillPipeAsync(_socket, pipe.Writer, _cancellationTokenSource.Token);
        Task reading = ReadPipeAsync(pipe.Reader, _cancellationTokenSource.Token);
    }

    public async Task Tx<T>(TelepathMessage<T> telepathMessage)
    {
        _logger?.LogTrace($"Called Tx<T>");
        if (_isFini)
        {
            _logger?.LogTrace("Tx isFini, throwing exception");
            throw new SynsharpException();
        }

        byte[] bytes = MessagePackSerializer.Serialize(telepathMessage);
        _logger?.LogTrace($"Sending message ({bytes.Length} bytes)");
        var bytesSent = await _socket.SendAsync(bytes, SocketFlags.None, _cancellationTokenSource.Token);
        _logger?.LogTrace($"Sent {bytesSent} bytes");
    }

    public async Task<dynamic> Rx()
    {
        _logger?.LogTrace($"Called Rx");
        if (_isFini)
        {
            _logger?.LogTrace("Rx isFini, throwing exception");
            throw new SynsharpException();
        }

        return await _rxqu.Reader.ReadAsync(_cancellationTokenSource.Token);
    }
    
    async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken)
    {
        while (!_isFini)
        {
            try 
            {
                _logger?.LogTrace("Starting pipeline reading loop");
                using (var stream = reader.AsStream())
                using (var streamReader = new MessagePackStreamReader(stream))
                {
                    ReadOnlySequence<byte>? readAsync;
                    while ((readAsync = await streamReader.ReadAsync(cancellationToken)) != null && !_isFini)
                    {
                        var msgBlock = (ReadOnlySequence<byte>)readAsync;
                        _logger?.LogTrace($"Pushing a new message to the queue (len: {msgBlock.Length})");
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
                _logger?.LogError(ex, $"Error in ReadPipeAsync: {ex.Message}");
                Finish();
                break;
            }
        }

        _logger?.LogTrace("Stopping pipeline reading loop");
        await reader.CompleteAsync();
    }
    
    async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken cancellationToken)
    {
        const int minimumBufferSize = 512;

        while (!_isFini)
        {
            _logger?.LogTrace("Starting pipeline filling loop");
            // Allocate at least 512 bytes from the PipeWriter
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);
            try 
            {
                int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                if (bytesRead == 0)
                {
                    _logger?.LogTrace("Read 0 bytes, done.");
                    Finish();
                    break;
                }

                _logger?.LogTrace($"Read {bytesRead} bytes");
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
                _logger?.LogError(ex, $"Error in FillPipeAsync: {ex.Message}");
                Finish();
                break;
            }

            _logger?.LogTrace($"Flush.");
            // Make the data available to the PipeReader
            FlushResult result = await writer.FlushAsync(cancellationToken);

            if (result.IsCompleted)
            {
                _logger?.LogTrace($"Complete, breaking.");
                break;
            }
        }

        _logger?.LogTrace("Stopping pipeline filling loop");
        await writer.CompleteAsync();
    }

    public void Finish()
    {
        if (_isFini)
            return;

        _isFini = true;
        
        _logger?.LogTrace("Closing link");
        _cancellationTokenSource.Cancel();
        if (_socket != null)
        {
            _logger?.LogTrace("Disposing socket");
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

        OnFini?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Finish();
        _logger?.LogTrace("Disposing link");
    }
}