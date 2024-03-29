using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Synsharp.Telepath.Messages;

namespace Synsharp.Telepath;

public class Proxy : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<string, Action<Dictionary<string, object>>> _handlers = new();
    private readonly ConcurrentQueue<Link> _links;
    private readonly ILogger? _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ProxyOptions? _options;

    private readonly Dictionary<string, TelepathTask> _tasks = new();
    
    private string? _iden; // TODO
    private Link? _link;
    private string _sess;
    private dynamic _sharinfo;

    private Proxy(Link link, ProxyOptions? options, ILoggerFactory? loggerFactory = null)
    {
        _link = link;
        _link.OnFini += OnLinkFini;
        
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory?.CreateLogger<Proxy>();

        _links = new ConcurrentQueue<Link>();
        LinkPoolSize = 4;

        _handlers.Add("task:fini", OnTaskFini);

        _cancellationTokenSource = new CancellationTokenSource();
    }

    public dynamic Methinfo { get; private set; }
    public int LinkPoolSize { get; set; }

    public byte[] SynVers => ((object[])_sharinfo["syn:version"]).Select(_ => (System.Byte) _).ToArray();
    public string SynCommit => (string)_sharinfo["syn:commit"];
    public string[] Classes => _sharinfo["classes"].Cast<string>().ToArray();
    public bool IsFini { get; private set; }

    public void Dispose()
    {
        _logger?.LogTrace("Disposing proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        _link?.Dispose();
        foreach (var link in _links) link.Dispose();
    }

    public async Task<T?> Task<T>(Todo todo, string? name)
    {
        if ((_link == null || _link.IsFini) | IsFini)
            throw new TelepathIsFini("Telepath Proxy isfini");

        if (!string.IsNullOrEmpty(_sess))
            return await TaskV2(todo, name);

        var task = new TelepathTask<T>();

        _tasks[task.iden] = task;

        var mesg = new TelepathMessage<TaskInitRequest>
        {
            Type = "task:init",
            Data = new TaskInitRequest
            {
                Task = task.iden,
                Todo = todo,
                Name = name
            }
        };

        var retn = default(T);
        try
        {
            await _link!.Tx(mesg);
            _logger?.LogTrace("waiting for result on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
            retn = await task.result();
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error {ErrorMessage} on proxy {ProxyId}", e.Message, this.GetHashCode().ToString("X4"));
            throw;
        }
        finally
        {
            _tasks.Remove(task.iden);
        }

        return retn;
    }

    public event EventHandler? OnProxyFini;

    private async Task<Link> GetPoolLink()
    {
        // Try to get a link from the queue, if none exists, create a new one. The new link should be added to the
        // pool when finish using.
        while (_links.TryDequeue(out var link))
        {
            if (link.IsFini) continue;
            return link;
        }

        return await InitPoolLink();
    }

    private async Task<Link> InitPoolLink()
    {
        var link = await Link.Connect(_link.Host, _link.Port, _link.LinkInfo, _loggerFactory?.CreateLogger<Link>());
        if (link.IsFini) OnLinkFini(null, null);
        return link;
    }

    private void OnLinkFini(object? sender, EventArgs e)
    {
        _logger?.LogTrace("A link of proxy {ProxyId} was finished, finishing the proxy", this.GetHashCode().ToString("X4"));
        FinishProxy();
    }

    private void FinishProxy()
    {
        if (IsFini)
        {
            _logger?.LogTrace("Proxy {ProxyId} is already finished", this.GetHashCode().ToString("X4"));
            return;
        }

        _logger?.LogTrace("Finishing proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        
        IsFini = true;
        if (OnProxyFini != null)
        {
            _logger?.LogTrace("Invoke OnFini handlers on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
            OnProxyFini?.Invoke(this, EventArgs.Empty);   
        }
        else
        {
            _logger?.LogTrace("No OnFini handlers on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        }

        _logger?.LogTrace("Terminating all tasks");
        var mesg = new object[]
        {
            "task:fini", new Dictionary<object, object>
            {
                { "retn", new object[] { false, new object[] { "IsFini", new Dictionary<object, object>() } } }
            }
        };
        foreach (var task in _tasks.Values) task.Reply(mesg);

        _logger?.LogTrace("Closing all links on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        foreach (var link in _links) link.Finish();

        _logger?.LogTrace("Closing base link on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        _link?.Finish();
    }

    private void OnTaskFini(Dictionary<string, object> data)
    {
        // [ "task:fini", { "task": "5d7b96e6-89de-4b4d-9d9f-3747f7919c28", "retn": [ true, 1 ] } ]

        var iden = (string)data["task"]!;
        if (_tasks.ContainsKey(iden))
        {
            if (!data.ContainsKey("type") || data["type"] == null)
            {
                var task = _tasks[iden];
                task.Reply((object[]?)data["retn"]);
            }
        }
        else
        {
            _logger?.LogWarning("task:fini for invalid task: {TaskIden} on proxy {ProxyId}", iden, this.GetHashCode().ToString("X4"));
        }
    }

    public static Task<Proxy> OpenUrlAsync(string url, ProxyOptions? options = null,
        ILoggerFactory? loggerFactory = null)
    {
        return OpenUrlAsync(new Uri(url), options, loggerFactory);
    }

    public static async Task<Proxy> OpenUrlAsync(Uri url, ProxyOptions? options = null,
        ILoggerFactory? loggerFactory = null)
    {
        var logger = loggerFactory?.CreateLogger<Proxy>();
        logger?.LogTrace("Called OpenUrlAsync for a new proxy");
        var scheme = url.Scheme;

        if (scheme == "aha")
        {
            throw new NotImplementedException("AHA urls are not (yet) supported");
        }
        
        if (scheme.Contains("+")) {
            var splittedScheme = scheme.Split('+', 1);
            if (splittedScheme.Length > 1 && splittedScheme[1] == "consul")
            {
                throw new NotSupportedException("Consul is no longer supported.");

            }
            else
            {
                throw new Exception("Unknown discovery protocol {scheme}");
            }
        }

        var host = url.Host;
        var port = url.Port;

        string username = null;
        string password = null;

        LinkInfo linkInfo = null;
        Auth auth = null;
        if (url.UserInfo != null)
        {
            var userinfo = url.UserInfo.Split(':', 2);
            username = userinfo.Length >= 1 ? userinfo[0] : string.Empty;
            password = userinfo.Length >= 2 ? userinfo[1] : string.Empty;

            auth = new Auth { User = username, Params = new Dictionary<string, string> { { "passwd", password } } };
        }

        Link link;
        if (scheme == "cell")
        {
            throw new NotImplementedException("Schema cell:// is not supported");
        }
        else if (scheme == "unix")
        {
            throw new NotImplementedException("Schema unix:// is not supported");
        }
        else
        {
            var path = url.AbsolutePath;
            var name = Path.GetPathRoot(path);

            if (port == null)
                port = 27492;

            if (scheme == "ssl")
            {
                var certDir = options.CertDir;
                var certHash = "";
                var certName = options.CertName;

                while (!File.Exists(Path.Combine(certDir, "cas", $"{host}.crt")))
                {
                    logger?.LogTrace("Testing for {CRTFile} failed", Path.Combine(certDir, "cas", $"{host}.crt"));
                    if (host.Contains("."))
                    {
                        var split = host.Split('.', 2);
                        if (split.Length > 1) host = split[1];
                    }
                    else
                    {
                        throw new SynsharpException("Could not find CA certifcate");
                    }
                }
                logger?.LogTrace("Testing for {CRTFile} succeed", Path.Combine(certDir, "cas", $"{host}.crt"));
                
                var hostname = options.HostName ?? host;
                
                // if a TLS connection specifies a user with no password attempt to auto-resolve a user certificate for the given host/network.
                if (string.IsNullOrEmpty(certName) && !string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                {
                    certName = $"{username}@{hostname}";
                }

                logger?.LogTrace("Creating linkInfo");
                linkInfo = new LinkInfo();
                if (string.IsNullOrEmpty(certHash))
                {
                    logger?.LogTrace("Setup X509Certificate from files");
                    var certificate = X509Certificate2.CreateFromPemFile(
                            Path.Combine(certDir, "users", $"{certName}.crt"), 
                            Path.Combine(certDir, "users", $"{certName}.key"));
                    linkInfo.clientCertificates = new X509CertificateCollection(new X509Certificate[] { certificate });
                    logger?.LogTrace("Setup complete for new proxy with SSL certificates");
                }
                else
                {
                    throw new NotSupportedException();
                }
                
                logger?.LogTrace("SSL Init is complete");
            }

            logger?.LogTrace("Request a new base link");
            link = await Link.Connect(host, port, linkInfo, loggerFactory?.CreateLogger<Link>());
        }
        
        var proxy = new Proxy(link, options, loggerFactory);
        logger?.LogTrace("Handshake for proxy {ProxyId}", proxy.GetHashCode().ToString("X4"));
        await proxy.Handshake(auth);
        logger?.LogTrace("Handshake for proxy {ProxyId} complete", proxy.GetHashCode().ToString("X4"));
        return proxy;
    }

    private async Task<dynamic?> Handshake(Auth? auth = null)
    {
        if (_link == null) throw new SynsharpException("Link is not connected");

        _logger?.LogTrace("Proxy starts authentication on proxy {ProxyId}", this.GetHashCode().ToString("X4"));

        var obj = new TelepathMessage<TeleSynRequest>
        {
            Type = "tele:syn",
            Data = new TeleSynRequest
            {
                Auth = auth,
                Vers = new[] { 3, 0 },
                Name = ""
            }
        };
        _logger?.LogTrace("TeleSynRequest: " + JsonConvert.SerializeObject(obj));

        _logger?.LogTrace("Send authentication request on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        await _link.Tx(obj);

        _logger?.LogTrace("Wait for reply from server on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
        var res = await _link.Rx();
        if (res == null)
        {
            _logger?.LogError("socket closed by server before handshake on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
            throw new SynsharpException("socket closed by server before handshake");
        }

        if (res[1]["retn"][0])
        {
            _logger?.LogTrace("Proxy {ProxyId} is now authenticated", this.GetHashCode().ToString("X4"));

            _sess = res[1]["sess"];
            _sharinfo = res[1]["sharinfo"];
            Methinfo = _sharinfo["meths"];
            
            _logger?.LogTrace("Proxy {ProxyId} received session {SessionId}", this.GetHashCode().ToString("X4"), _sess);

            _logger?.LogTrace(string.Join("\n",
                    ((Dictionary<object, object>)Methinfo).Select(test =>
                        test.Key + " =>  " + ((bool)((Dictionary<object, object>)test.Value)["genr"] ? "gen" : "-"))));

            var data = res[1];
            if (!obj.Data.Vers.SequenceEqual(((object[])data["vers"]).Select(System.Convert.ToInt32)))
                throw new BadVersionException();

            _logger?.LogTrace("Proxy {ProxyId} starts receive loop", this.GetHashCode().ToString("X4"));
#pragma warning disable CS4014
            // This loop runs in the background and should not be awaited.
            System.Threading.Tasks.Task.Run(RxLoop);
#pragma warning restore CS4014
            return Common.Result(data["retn"]);
        }
        else
        {
            _logger?.LogTrace("Proxy {ProxyId} could not authenticate", this.GetHashCode().ToString("X4"));
            if (res[1]["retn"][1][1].ContainsKey("mesg"))
                throw new SynsharpException(res[1]["retn"][1][0].ToString() + ": " + res[1]["retn"][1][1]["mesg"].ToString());
            else
                throw new SynsharpException(res[1]["retn"][1][0].ToString());
        }
    }

    private async Task RxLoop()
    {
        while (_link is { IsFini: false })
        {
            var mesg = await _link.Rx();
            if (mesg == null) return;

            try
            {
                string s = mesg.Type.ToString();
                _logger?.LogTrace("Received message '{Message}' on proxy {ProxyId}", s, this.GetHashCode().ToString("X4"));
                
                if (_handlers.ContainsKey(mesg[0]))
                    _handlers[mesg.Type](mesg[1]);
                else
                    throw new NotImplementedException("{mesg.Type} has no handler");
            }
            catch (Exception e)
            {
                _logger?.LogError("RxLoop: {ErrorMessage} on proxy {ProxyId}", e.Message, this.GetHashCode().ToString("X4"));
                return;
            }
        }

        _logger?.LogTrace("Terminating RxLoop on proxy {ProxyId}", this.GetHashCode().ToString("X4"));
    }

    private dynamic? Call(string name, object[] args, Dictionary<string, object?> kwargs)
    {
        var meth = new Method(this, name, _iden);
        return meth.Call(args, kwargs);
    }

    private IAsyncEnumerable<dynamic?> Stream(string name, object[] args, Dictionary<string, object?> kwargs)
    {
        var meth = new Method(this, name, _iden);
        return meth.Stream(args, kwargs);
    }

    public async Task<dynamic?> TaskV2(Todo todo, string? name = null)
    {
        var mesg = new TelepathMessage<T2InitRequest>
        {
            Type = "t2:init",
            Data = new T2InitRequest
            {
                Todo = todo,
                Name = name,
                Sess = _sess
            }
        };

        var link = await GetPoolLink();
        if (link == null || link.IsFini)
        {
            FinishProxy();
            throw new Exception("Could not get link from pool link");
        }
        await link.Tx(mesg);

        var resp = await link.Rx();
        if (resp == null) throw new ArgumentNullException(nameof(resp));

        if (resp[0] == "t2:fini")
        {
            await PutPoolLink(link);
            var data = resp[1];
            var retn = (object[])data["retn"];
            return Common.Result(retn);
        }

        if (resp[0] == "t2:genr")
            throw new SynsharpException("Use stream method for methods returning multiple nodes");
        if (resp[0] == "t2:share") throw new NotImplementedException();

        return default;
    }


    public async IAsyncEnumerable<dynamic?> TaskV2Genr(Todo todo, string? name = null)
    {
        var mesg = new TelepathMessage<T2InitRequest>
        {
            Type = "t2:init",
            Data = new T2InitRequest
            {
                Todo = todo,
                Name = name,
                Sess = _sess
            }
        };

        var link = await GetPoolLink();
        await link.Tx(mesg);

        var resp = await link.Rx();
        if (resp == null) throw new ArgumentNullException(nameof(resp));

        if (resp[0] == "t2:fini")
        {
            await PutPoolLink(link);
            if (resp[1]["retn"][0] is bool b && b)
            {
                var data = resp.Data;
                var retn = (object[])data["retn"];
                yield return Common.Result(retn);
            }
            else
            {
                throw new SynsharpError("t2:fini returned false");
            }
        }
        else if (resp[0] == "t2:genr")
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    resp = await link.Rx();
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"Got error: {e.Message}");
                    link.Dispose();
                    throw;
                }

                if (resp == null) throw new ArgumentNullException(nameof(resp));

                if (resp[0] != "t2:yield")
                    throw new TelepathBadMessage("Telepath protocol violation:  unexpected message received");

                var data = resp[1];
                var retn = data["retn"];
                if (retn == null)
                {
                    await PutPoolLink(link);
                    break;
                }

                if (!(bool)retn[0])
                    await PutPoolLink(link);

                yield return Common.Result(retn);
            }
        }
        else if (resp.Type == "t2:share")
        {
            throw new NotImplementedException();
        }
    }

    private Task PutPoolLink(Link link)
    {
        if (IsFini)
        {
            if (_logger != null) _logger.LogError($"Not putting link back in pool: {_links.Count}");
            return System.Threading.Tasks.Task.CompletedTask;
        }

        if (_links.Count > LinkPoolSize)
            link.Dispose();
        else
            _links.Enqueue(link);
        _logger?.LogTrace($"Size of pool link: {_links.Count}");
        
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async Task<long> Count(string query, StormOps? opts = null)
    {
        var args = new object[] { query };
        var kwargs = new Dictionary<string, object?>()
        {
            { "opts", opts }
        };
        return (long) await Call("count", args, kwargs);
    }

    public async Task<dynamic?> CallStormAsync(string query, StormOps? opts = null)
    {
        var args = new object[] { query };
        var kwargs = new Dictionary<string, object?>()
        {
            { "opts", opts }
        };
        return await Call("callStorm", args, kwargs);
    }
    
    public async Task<T?> CallStormAsync<T>(string query, StormOps? opts = null)
    {
        var result = await CallStormAsync(query, opts);
        var serializeObject = JsonConvert.SerializeObject(result);
        return JsonConvert.DeserializeObject<T>(serializeObject);
    }

    public async IAsyncEnumerable<SynapseMessage?> Storm(string query, StormOps? opts = null)
    {
        var args = new object[] { query };
        var kwargs = new Dictionary<string, object?>()
        {
            { "opts", opts }
        };
        await foreach (var m in Stream("storm", args, kwargs))
        {
            dynamic message;
            try
            {
                message = SynConvert.ToMessage(m);
            }
            catch (Exception e)
            {
                _logger?.LogError($"Could not convert message: {JsonConvert.SerializeObject(m)}");
                yield break;
            }
            yield return message;
        }
    }
}