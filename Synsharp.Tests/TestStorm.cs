using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

namespace Synsharp.Tests;

public class TestStorm: TestSynapse
{
    [Test]
    public async Task TestEmptyQuery()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<SynapseObject>("")
            .ToListAsync();
        
        Assert.AreEqual(0, response.Count());
    }
    
    [Test]
    public async Task TestPreloadIden()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<SynapseObject>("[ inet:ipv4=8.8.8.8 ]")
            .ToListAsync();
        Assert.AreEqual(1, response.Count());
        var ip = response.First();
        
        response = await SynapseClient
            .StormAsync<SynapseObject>("", new ApiStormQueryOpts()
            {
                Idens = new string [] { ip.Iden }
            })
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());
        Assert.AreEqual(InetIPv4.Parse("8.8.8.8"), response.Single());
        Assert.AreEqual(ip.Iden, response.Single().Iden);
    }
}