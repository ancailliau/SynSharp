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
                _logger?.LogTrace($"Successfully connected to {UrlHelper.SanitizeUrl(url)}");
                Ready.Set();
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
        _logger?.LogTrace("Calling InitTeleLink {URL}", url);
        _proxy = await Proxy.OpenUrlAsync(url, _opts, _loggerFactory);
        // TODO What if proxy is null
        
        _logger?.LogTrace("Got a new proxy: {ProxyId}", _proxy.GetHashCode().ToString("X4"));
        _methInfo = _proxy.Methinfo;
        _proxy.LinkPoolSize = _configuration.LinkPoolSize;
        _proxy.OnProxyFini += OnProxyFini;
        
        try
        {
            OnLink(EventArgs.Empty);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Onlink handler failed on client: {ErrorMessage}", e.Message);
        }
    }

    private void OnProxyFini(object? sender, EventArgs eventArgs)
    {
        if (!IsFini)
        {
            _logger?.LogTrace("Proxy {ProxyId} is finished, requesting a new one", _proxy.GetHashCode().ToString("X4"));
            FireLinkLoop();
        }
        else 
            _logger?.LogTrace("Proxy {ProxyId} is finished but so is the client, do nothing.", _proxy.GetHashCode().ToString("X4"));
    }

    private void FireLinkLoop()
    {
        _logger?.LogTrace("FireLinkLoop");
        _proxy = null;
        Ready.Clear();
        System.Threading.Tasks.Task.Run(TeleLinkLoop);
    }

    public bool IsFini { get; set; } = false;

    private AsyncAutoResetEvent Ready { get; }
   
    public void Dispose()
    {
        IsFini = true;
        _logger?.LogTrace("Disposing TelepathClient");
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
        if (timeout == null)
        {
            _logger?.LogTrace("Will wait indefinitely");
            var task = Ready.WaitAsync();
            _logger?.LogTrace("Waiting for task {TaskId} to be completed", task.GetHashCode().ToString("X4"));
            task.Wait();
            _logger?.LogTrace("Task {TaskId} completed: {TaskCompleted}", task.GetHashCode().ToString("X4"), task.IsCompleted);
        }
        else
        {
            _logger?.LogTrace("Will wait for maximum {Timeout}", timeout);
            await System.Threading.Tasks.Task.WhenAny(Ready.WaitAsync(),
                System.Threading.Tasks.Task.Delay((TimeSpan)timeout));
        }
    }

    public async Task<Proxy> GetProxyAsync(TimeSpan? timeout = null)
    {
        await WaitReady(timeout);
        
        if (_proxy != null)
        {
            if (!_proxy.IsFini) return _proxy;
            throw new SynsharpException($"Client got finished proxy {_proxy.GetHashCode().ToString("X4")}");
        }

        throw new SynsharpException($"Client could not get a proxy");
    }
    
    protected virtual void OnLink(EventArgs e)
    {
        OnLinked?.Invoke(this, e);
    }
}