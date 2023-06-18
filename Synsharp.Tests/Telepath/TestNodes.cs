using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;
using Synsharp.Telepath;
using Synsharp.Telepath.Messages;

namespace Synsharp.Tests.Telepath;

public class TestNodesTelepath : TestTelepath
{
    [Test]
    public async Task TestAddIPv6()
    {
        Assert.NotNull(SynapseClient);

        var proxy = await SynapseClient.GetProxyAsync();
        
        var response = await proxy.Storm("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]", new StormOps() {Repr = true})
            .OfType<SynapseNode>()
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(response.First().Valu.Equals("2001:db8:85a3::8a2e:370:7334"));
    }
    
    [Test]
    public async Task TestGetIPv6()
    {
        Assert.NotNull(SynapseClient);
        var proxy = await SynapseClient.GetProxyAsync();
        
        _ = await proxy.Storm("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]", new StormOps() {Repr = true})
            .OfType<SynapseNode>()
            .ToListAsync();
        
        var response = await proxy.Storm("inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334", new StormOps() {Repr = true})
            .OfType<SynapseNode>()
            .ToListAsync();
        Assert.That(response.First().Valu.Equals("2001:db8:85a3::8a2e:370:7334"));
    }

    [Test]
    public async Task TestAddIPv4()
    {
        Assert.NotNull(SynapseClient);
        
        var proxy = await SynapseClient.GetProxyAsync();
        
        var response = await proxy.Storm("[ inet:ipv4=8.8.8.8 ]", new StormOps() {Repr = true})
            .OfType<SynapseNode>()
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Repr.Equals("8.8.8.8"));
    }
    
    [Test]
    public async Task TestAddInetUrl()
    {
        Assert.NotNull(SynapseClient);
        
        var proxy = await SynapseClient.GetProxyAsync();
        
        var response = await proxy.Storm("[ inet:url=\"http://www.example.org/files/index.html\" ]")
            .OfType<SynapseNode>()
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Props["base"].ToString().Equals(Types.Str.Parse("http://www.example.org/files/index.html")), $"Expected 'http://www.example.org/files/index.html' but got '{first.Props["base"]}' "); 
        Assert.That(first.Props["fqdn"].ToString().Equals(Types.InetFqdn.Parse("www.example.org")), $"Expected 'www.example.org' but got '{first.Props["fqdn"]}' ");
    }

    [Test]
    public async Task TestAddInetUrlWithUsernameAndPort()
    {
        Assert.NotNull(SynapseClient);

        var url = "http://john.doe:evil@www.example.org:1234/files/index.html?param=evilcorp"; 
        
        var proxy = await SynapseClient.GetProxyAsync();
        
        await proxy.CallStormAsync($"inet:url=\"{url}\" | delnode");
        
        var response = await proxy.Storm($"[ inet:url=\"{url}\" ]")
            .OfType<SynapseNode>()
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Props["fqdn"].ToString().Equals(Types.InetFqdn.Convert("www.example.org")), $"Expected 'www.example.org' but got '{first.Props["fqdn"]}' ");
        Assert.That(first.Props["user"].ToString().Equals(Types.InetUser.Convert("john.doe")), $"Expected 'john.doe' but got '{first.Props["user"]}' ");
        Assert.That(first.Props["passwd"].ToString().Equals(Types.InetPasswd.Convert("evil")), $"Expected 'evil' but got '{first.Props["passwd"]}' ");
        Assert.That(first.Props["port"].Equals((UInt16)1234), $"Expected '1234' but got '{first.Props["port"]}' ");
    }

    [Test]
    public async Task TestAddInetEmail()
    {
        Assert.NotNull(SynapseClient);
        
        var proxy = await SynapseClient.GetProxyAsync();
        var response = await proxy.Storm("[ inet:email=\"john.doe@example.org\" ]")
            .OfType<SynapseNode>()
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Props["fqdn"].ToString().Equals(Types.InetFqdn.Convert("example.org")), $"Expected 'example.org' but got '{first.Props["fqdn"]}' ");
        Assert.That(first.Props["user"].ToString().Equals(Types.InetUser.Convert("john.doe")), $"Expected 'john.doe' but got '{first.Props["user"]}' ");
    }

    [Test]
    public async Task TestAddX509()
    {
        Assert.NotNull(SynapseClient);
        
        var proxy = await SynapseClient.GetProxyAsync();
        var response = await proxy.Storm("[ crypto:x509:cert=* :md5=ebff56c59290e26d64050e0b68ec6575 ]")
            .OfType<SynapseNode>()
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());
        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.AreEqual("ebff56c59290e26d64050e0b68ec6575", first.Props["md5"].ToString());
    }
    
    
}