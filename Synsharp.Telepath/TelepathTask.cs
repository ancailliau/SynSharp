namespace Synsharp.Telepath;

public class TelepathTask<T> : TelepathTask
{
    public async Task<T> result()
    {
        return (T) await _tcs.Task;
    }
}

public abstract class TelepathTask
{
    public string iden = Guid.NewGuid().ToString(); // Good?
    protected readonly TaskCompletionSource<object> _tcs;

    public TelepathTask()
    {
        _tcs = new TaskCompletionSource<object>();
    }
    
    public void Reply(object[]? reply)
    {
        _tcs.SetResult(Common.Result(reply));
    }
}

public static class Common
{
    public static dynamic? Result(dynamic reply)
    {
        if (reply == null) return null;
        var ok = (bool) reply[0];
        if (ok)
        {
            return reply[1];
        }
        else
        {
            var error = (object[]) reply[1];
            var details = (Dictionary<object, object>)error[1];
            if (details.ContainsKey("mesg")) throw new Exception($"{error[0]}: {details["mesg"]}");
            else throw new Exception($"{error[0]}");
        }
    }
}