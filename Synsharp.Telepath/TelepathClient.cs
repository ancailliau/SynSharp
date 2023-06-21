using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath;

public class TelepathClient : IDisposable
{
    private readonly string[] _urls;
    private Queue<string>? _urlsQueue;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;
    
    private readonly ClientConfiguration _configuration;
    
    private Proxy? _proxy;
    private ProxyOptions? _opts;
    private dynamic _methInfo;

    public event EventHandler OnLinked;

    public TelepathClient(string url, ProxyOptions? opts = null, ClientConfiguration? configuration = null,
        ILoggerFactory? loggerFactory = null) : this(new[] { url }, opts, configuration, loggerFactory)
    {
        
    }
    
    public TelepathClient(string[] urls, ProxyOptions? opts = null, ClientConfiguration? configuration = null,
        ILoggerFactory? loggerFactory = null)
    {
        _configuration = configuration ?? new ClientConfiguration();
        _opts = opts ?? new ProxyOptions();
        _urls = urls;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger<TelepathClient>();
        Ready = new AsyncAutoResetEvent(loggerFactory?.CreateLogger<AsyncAutoResetEvent>());

        FireLinkLoop();
    }

    private async Task TeleLinkLoop()
    {
        var lastLog = 0L;
        while (!IsFini)
        {
            var url = GetNextUrl();
            _logger?.LogTrace($"Will try to connect to {UrlHelper.SanitizeUrl(url)}");
            try
            {
                await InitTeleLink(url);
                Ready.Set();
                _logger?.LogTrace($"Successfully connected to {UrlHelper.SanitizeUrl(url)}");
                return;
            }
            // TODO: Redirect - self._setNextUrl(e.errinfo.get('url'))
            catch (SynsharpException e)
            {
                _logger?.LogError($"telepath client ({UrlHelper.SanitizeUrl(url)}) encountered an error ({e.GetType().Name}): {e.Message}\n{e.StackTrace}");
                Ready.Set();
                IsFini = true;
                return;
            }
            catch (Exception e)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now > lastLog + 60L)
                {
                    _logger?.LogError($"telepath client ({UrlHelper.SanitizeUrl(url)}) encountered an error ({e.GetType().Name}): {e.Message}\n{e.StackTrace}");
                    lastLog = now;
                }
            }

            _logger?.LogTrace($"Loop will sleep for {_configuration.RetrySleep} ms");
            await System.Threading.Tasks.Task.Delay(_configuration.RetrySleep);
        }
    }

    private string GetNextUrl()
    {
        if (_urlsQueue == null || !_urlsQueue.Any())
            InitUrlQueue();
        return _urlsQueue.Dequeue();
    }

    private void InitUrlQueue()
    {
        _urlsQueue = new Queue<string>(_urls);
    }

    private void SetNextUrl(string url)
    {
        _urlsQueue.Enqueue(url);
    }

    private async Task InitTeleLink(string url)
    {
        _proxy = await Proxy.OpenUrlAsync(url, _opts, _loggerFactory);
        _methInfo = _proxy.Methinfo;
        _proxy.LinkPoolSize = _configuration.LinkPoolSize;
        _proxy.OnFini += OnProxyFini; 
        
        try
        {
            OnLink(EventArgs.Empty);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"onlink: {e.Message}");
        }
    }

    private void OnProxyFini(object? sender, EventArgs eventArgs)
    {
        _logger?.LogTrace("Proxy is finished");
        FireLinkLoop();
    }

    private void FireLinkLoop()
    {
        _logger?.LogTrace("FireLinkLoop");
        _proxy = null;
        Ready.Clear();
        System.Threading.Tasks.Task.Run(TeleLinkLoop);
    }

    public bool IsFini { get; set; } = false;

    private AsyncAutoResetEvent Ready { get; init; }
   
    public void Dispose()
    {
        _proxy?.Dispose();
        
        // Avoid tasks without timeouts specified to wait forever
        Ready.Set();
    }

    public async Task<T?> Task<T>(Todo todo, string? name)
    {
        while (!IsFini)
        {
            try
            {
                await WaitReady();
                return await _proxy.Task<T>(todo, name);
            }
            // TODO: Redirect - self._setNextUrl(e.errinfo.get('url'))
            catch (Exception e)
            {
                _logger?.LogError(e, "Error while executing a task.");
                throw;
            }
        }
        _logger?.LogDebug("Client IsFini");
        throw new SynsharpException();
    }

    private async Task WaitReady(TimeSpan? timeout = null)
    {
        if (timeout != null)
            await System.Threading.Tasks.Task.WhenAny(Ready.WaitAsync(), System.Threading.Tasks.Task.Delay((TimeSpan)timeout));
        else
            await Ready.WaitAsync();
    }

    public async Task<Proxy> GetProxyAsync(TimeSpan? timeout = null)
    {
        await WaitReady(timeout);
        if (_proxy != null && !_proxy.IsFini) return _proxy;
        throw new SynsharpException($"Could not get proxy {timeout}");
    }
    
    protected virtual void OnLink(EventArgs e)
    {
        OnLinked?.Invoke(this, e);
    }
}