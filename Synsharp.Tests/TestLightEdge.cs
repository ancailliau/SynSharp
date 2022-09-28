using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

namespace Synsharp.Tests;

public class TestLightEdge : TestSynapse
{
    [Test]
    public async Task TestAdd()
    {
        var ip1 = new InetIPv4("8.8.8.8");
        var ip2 = new InetIPv4("9.9.9.9");

        ip1 = await SynapseClient.Nodes.Add<InetIPv4>(ip1);
        ip2 = await SynapseClient.Nodes.Add<InetIPv4>(ip2);
        await SynapseClient.Nodes.AddLightEdge(ip1, ip2, "refs");

        var relatedIps = await (SynapseClient.StormAsync<object>("inet:ipv4=8.8.8.8 <(refs)- *")).ToListAsync();
        Assert.AreEqual(1, relatedIps.Count);
        Assert.That(relatedIps.First().GetType() == typeof(InetIPv4), 
            $"Returned value is type '{relatedIps.First().GetType().FullName}' and not InetIPv4");
        var relatedIp = (InetIPv4)relatedIps.First();
        Assert.That(relatedIp.Equals(InetIPv4.Parse("9.9.9.9")), 
            $"Returned value is not 9.9.9.9 but {relatedIp.Value.ToString()}");
    }
    
    [Test]
    public async Task TestAddMultiple()
    {
        var ip1 = new InetIPv4("8.8.8.8");
        var ip2 = new InetIPv4("9.9.9.9");

        var fqdn1 = new InetFqdn("dns1.google.com");
        var fqdn2 = new InetFqdn("dns2.google.com");

        ip1 = await SynapseClient.Nodes.Add<InetIPv4>(ip1);
        ip2 = await SynapseClient.Nodes.Add<InetIPv4>(ip2);

        var tuples = new SynapseLightEdge[]
        {
            new(ip1, ip2, "refs"),
            new(fqdn1, fqdn2, "refs"),
        };
        
        await SynapseClient.Nodes.AddLightEdge(tuples);

        var relatedIps = await (SynapseClient.StormAsync<object>("inet:ipv4=8.8.8.8 <(refs)- *")).ToListAsync();
        Assert.AreEqual(1, relatedIps.Count);
        Assert.That(relatedIps.First().GetType() == typeof(InetIPv4), 
            $"Returned value is type '{relatedIps.First().GetType().FullName}' and not InetIPv4");
        var relatedIp = (InetIPv4)relatedIps.First();
        Assert.That(relatedIp.Equals(InetIPv4.Parse("9.9.9.9")), 
            $"Returned value is not 9.9.9.9 but {relatedIp.Value.ToString()}");
    }
}