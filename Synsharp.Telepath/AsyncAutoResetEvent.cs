using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath;

public class AsyncAutoResetEvent
{
    // Thanks to https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
    private static readonly Task Completed = Task.FromResult(true);
    private readonly Queue<TaskCompletionSource<bool>> _waits = new();
    private bool _signaled;
    private ILogger? _logger;

    public AsyncAutoResetEvent(ILogger? logger = null)
    {
        _logger = logger;
    }

    public Task WaitAsync()
    {
        lock (_waits)
        {
            if (_signaled)
            {
                _logger?.LogTrace("AsyncAutoResetEvent already signaled, no need to wait");
                // In our case, we don't want to reset the signaled flag. It will be reset using Clear method.
                // _signaled = false;
                return Completed;
            }

            var tcs = new TaskCompletionSource<bool>();
            _waits.Enqueue(tcs);
            _logger?.LogTrace("Waiting for AsyncAutoResetEvent");
            return tcs.Task;
        }
    }
    
    public void Set()
    {
        TaskCompletionSource<bool> toRelease = null;
        // Release all the tasks waiting for the event
        lock (_waits)
        {
            while (_waits.Count > 0)
            {
                toRelease = _waits.Dequeue();
                if (toRelease != null) 
                    toRelease.SetResult(true);
            }
        }
        
        _signaled = true;
        _logger?.LogTrace($"Set AsyncAutoResetEvent: {_signaled}");
    }

    public void Clear()
    {
        _logger?.LogTrace("Clearing AsyncAutoResetEvent");
        _signaled = false;
    }
}