using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;
using InetIPv4 = Synsharp.Types.InetIPv4;

namespace Synsharp.Tests;

public class TestNodesComp : TestSynapse
{
    [Test]
    public async Task TestAddInetDNSA()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetDnsA>("[ inet:dns:a=(office-protection.online,93.95.228.74) ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.IPv4, Is.EqualTo(Types.InetIPv4.Parse("93.95.228.74")));
        Assert.That(first.FQDN, Is.EqualTo(Types.InetFqdn.Parse("office-protection.online")));
    }
    
    [Test]
    public async Task TestAddInetDNSAAnswer()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetDnsAnswer>("[ inet:dns:answer=* :ttl=300 :a=(office-protection.online,93.95.228.74) ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.A, Is.EqualTo(new Types.InetDnsA("office-protection.online", Types.InetIPv4.Parse("93.95.228.74"))));
        Assert.That(first.TTL, Is.EqualTo(300));
    }
    
    [Test]
    public async Task TestAddInetDNSAAnswerWithHelper()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .Nodes.Add(new InetDnsAnswer()
            {
                TTL = 300,
                A = new Types.InetDnsA("office-protection.online", InetIPv4.Parse("93.95.228.74"))
            });
        
        Assert.IsNotNull(response);
        Assert.That(response.A, Is.EqualTo(new Types.InetDnsA("office-protection.online", Types.InetIPv4.Parse("93.95.228.74"))));
        Assert.That(response.TTL, Is.EqualTo(300));
    }
}