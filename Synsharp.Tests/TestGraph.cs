using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Synsharp.Tests;

public class TestGraph
{
    private SynapseClient _client;
    
    // Disabled old test
    public async Task TestGetSubgraphForLightweightEdge()
    {
        Assert.NotNull(_client);
        
        var documents = await _client
            .StormAsync<object>("inet:ipv4=192.168.3.4 | graph --degrees 2 --pivot { <(refs)- * } --pivot { -(refs)> * }")
            .ToListAsync();
        foreach (var document in documents)
        {
            Console.WriteLine(document);
        }
    }
    
    // Disabled old test
    public async Task TestGetSubgraphForForm()
    {
        Assert.NotNull(_client);
        
        var documents = await _client
            .StormAsync<object>("inet:ipv4=8.8.8.8 | graph --degrees 2 --pivot { <- * } --pivot { -> * }")
            .ToListAsync();
        foreach (var document in documents)
        {
            Console.WriteLine(document);
        }
    }
}