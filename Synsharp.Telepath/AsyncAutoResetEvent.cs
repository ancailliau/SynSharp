using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath;

public class AsyncAutoResetEvent
{
    // Thanks to https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
    private static readonly Task Completed = Task.FromResult(true);
    
    private readonly Queue<TaskCompletionSource<bool>> _waits = new();
    private readonly ILogger? _logger;
    
    private bool _signaled;

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
            _logger?.LogTrace("Enqueued {TaskCompletionSourceId} on waits (len: {WaitListLen})", tcs.GetHashCode().ToString("X4"), _waits.Count);
            return tcs.Task;
        }
    }
    
    public void Set()
    {
        _logger?.LogTrace($"Set AsyncAutoResetEvent");
        // Release all the tasks waiting for the event
        lock (_waits)
        {
            _logger?.LogTrace($"Got the lock, will clear all the tasks");
            while (_waits.Count > 0)
            {
                var toRelease = _waits.Dequeue();
                if (toRelease != null) 
                    toRelease.SetResult(true);
            }
            _logger?.LogTrace($"All tasks are cleared");
        
            _signaled = true;
        }
    }

    public void Clear()
    {
        lock (_waits)
        {
            _logger?.LogTrace("Clearing AsyncAutoResetEvent");
            _logger?.LogTrace("Waits queue (len: {WaitListLen})", _waits.Count);
            _signaled = false;
        }
    }
}