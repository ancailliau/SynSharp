using System.Collections.Concurrent;
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
    private readonly Uri _url;

    private string? _iden; // TODO
    private Link? _link;
    private string _sess;
    private dynamic _sharinfo;

    private Proxy(Uri url, ProxyOptions? options, ILoggerFactory? loggerFactory = null)
    {
        _url = url;
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
        _logger?.LogTrace("Disposing proxy");
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
            _logger?.LogTrace("waiting for result");
            retn = await task.result();
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error: " + e.Message);
            throw;
        }
        finally
        {
            _tasks.Remove(task.iden);
        }

        return retn;
    }

    public event EventHandler? OnFini;

    private async Task<Link> GetPoolLink()
    {
        while (_links.TryDequeue(out var link))
        {
            if (link.IsFini) continue;
            return link;
        }

        return await InitPoolLink();
    }

    private async Task<Link> InitPoolLink()
    {
        var link = await Link.Connect(_url.Host, _url.Port, _loggerFactory?.CreateLogger<Link>());
        link.OnFini += OnLinkFini;
        return link;
    }

    private void OnLinkFini(object? sender, EventArgs e)
    {
        _logger?.LogTrace("OnLink Fini");
        Fini();
    }

    private void Fini()
    {
        if (IsFini)
            return;

        IsFini = true;
        OnFini?.Invoke(this, EventArgs.Empty);

        _logger?.LogTrace("Terminating all tasks");
        var mesg = new object[]
        {
            "task:fini", new Dictionary<object, object>
            {
                { "retn", new object[] { false, new object[] { "IsFini", new Dictionary<object, object>() } } }
            }
        };
        foreach (var task in _tasks.Values) task.Reply(mesg);

        _logger?.LogTrace("Closing all links");
        foreach (var link in _links) link.Finish();

        _logger?.LogTrace("Closing link");
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
            _logger?.LogWarning($"task:fini for invalid task: {iden}");
        }
    }

    private async Task ConnectAsync()
    {
        _link = await GetPoolLink();
        _link.OnFini += OnLinkFini;

        _logger?.LogTrace("Link connected");
    }

    public static Task<Proxy> OpenUrlAsync(string url, ProxyOptions? options = null,
        ILoggerFactory? loggerFactory = null)
    {
        return OpenUrlAsync(new Uri(url), options, loggerFactory);
    }

    public static async Task<Proxy> OpenUrlAsync(Uri url, ProxyOptions? options = null,
        ILoggerFactory? loggerFactory = null)
    {
        var proxy = new Proxy(url, options, loggerFactory);
        await proxy.ConnectAsync();
        await proxy.Handshake();
        return proxy;
    }

    private async Task<dynamic?> Handshake()
    {
        if (_link == null) throw new SynsharpException("Link is not connected");

        _logger?.LogTrace("Proxy starts authentication");

        // Send ["tele:syn",{"auth":["root",{"passwd":"secret"}],"vers":[3,0],"name":""}]
        var userinfo = _url.UserInfo.Split(':', 2);
        var username = userinfo.Length >= 1 ? userinfo[0] : string.Empty;
        var password = userinfo.Length >= 2 ? userinfo[1] : string.Empty;
        var obj = new TelepathMessage<TeleSynRequest>
        {
            Type = "tele:syn",
            Data = new TeleSynRequest
            {
                Auth = new Auth { User = username, Params = new Dictionary<string, string> { { "passwd", password } } },
                Vers = new[] { 3, 0 },
                Name = ""
            }
        };

        _logger?.LogTrace("Send authentication request");
        await _link.Tx(obj);

        _logger?.LogTrace("Wait for reply from server");
        var res = await _link.Rx();
        if (res == null) throw new SynsharpException("socket closed by server before handshake");

        _logger?.LogTrace("Proxy is now authenticated");

        _sess = res[1]["sess"];
        _sharinfo = res[1]["sharinfo"];
        Methinfo = _sharinfo["meths"];

        if (_logger != null)
            _logger.LogTrace(string.Join("\n",
                ((Dictionary<object, object>)Methinfo).Select(test =>
                    test.Key + " =>  " + ((bool)((Dictionary<object, object>)test.Value)["genr"] ? "gen" : "-"))));

        var data = res[1];
        if (!obj.Data.Vers.SequenceEqual(((object[])data["vers"]).Select(System.Convert.ToInt32)))
            throw new BadVersionException();

        _logger?.LogTrace("Proxy starts receive loop");
#pragma warning disable CS4014
        // This loop runs in the background and should not be awaited.
        System.Threading.Tasks.Task.Run(RxLoop);
#pragma warning restore CS4014

        return Common.Result(data["retn"]);
    }

    private async Task RxLoop()
    {
        while (_link is { IsFini: false })
        {
            var mesg = await _link.Rx();
            if (mesg == null) return;

            try
            {
                _logger?.LogTrace($"Received message '{mesg.Type.ToString()}'");
                if (_handlers.ContainsKey(mesg[0]))
                    _handlers[mesg.Type](mesg[1]);
                else
                    throw new NotImplementedException("{mesg.Type} has no handler");
            }
            catch (Exception e)
            {
                _logger?.LogError($"RxLoop: {e.Message}");
                return;
            }
        }

        _logger?.LogTrace("Terminating RxLoop");
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
            var data = resp.Data;
            var retn = (object[])data["retn"];
            yield return Common.Result(retn);
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
        if (IsFini) return System.Threading.Tasks.Task.CompletedTask;
        if (_links.Count > LinkPoolSize)
            link.Dispose();
        _links.Enqueue(link);
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
            yield return SynConvert.ToMessage(m);
    }
}