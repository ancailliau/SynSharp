using System.Collections.Generic;
using System.Threading.Tasks;

namespace Synsharp.Telepath;

public class Method
{
    private readonly Proxy _proxy;
    private readonly string _name;
    private readonly string? _share;

    public Method(Proxy proxy, string name, string? share = null)
    {
        _proxy = proxy;
        _name = name;
        _share = share;
    }

    public Task<dynamic?> Call(object[] args, Dictionary<string, object?> kwargs)
    {
        var todo = new Todo () { Name = _name, Args = args, KwArgs = kwargs};
        return _proxy.TaskV2(todo, _share);
    }

    public IAsyncEnumerable<dynamic?> Stream(object[] args, Dictionary<string, object?> kwargs)
    {
        var todo = new Todo () { Name = _name, Args = args, KwArgs = kwargs};
        return _proxy.TaskV2Genr(todo, _share);
    }
}