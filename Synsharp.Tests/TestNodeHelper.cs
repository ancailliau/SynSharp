using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;
using Synsharp.Types;
using CryptoX509Cert = Synsharp.Forms.CryptoX509Cert;
using InetFqdn = Synsharp.Forms.InetFqdn;
using InetIPv6 = Synsharp.Forms.InetIPv6;
using InetUrl = Synsharp.Forms.InetUrl;

namespace Synsharp.Tests;

public class TestNodeHelper : TestSynapse
{   
    [Test]
    public async Task TestAddMultipleNodes()
    {
        Assert.NotNull(SynapseClient);

        var response = SynapseClient
            .Nodes.Add(new SynapseObject[]
            {
                InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334"),
                InetFqdn.Parse("www.google.com"),
                InetUrl.Parse("http://example.org/dummy.html")
            });
        
        Assert.IsNotNull(response);
        Assert.That((await response.CountAsync()) == 3);
    }
    
    [Test]
    public async Task TestAddIPv6Node()
    {
        Assert.NotNull(SynapseClient);

        var response = await SynapseClient
            .Nodes.Add(InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334"));
        
        Assert.IsNotNull(response);
        Assert.That(response.Equals(InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334")));
    }
    
    [Test]
    public async Task TestAddNodeWithTags()
    {
        Assert.NotNull(SynapseClient);

        var inetIPv6 = InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        inetIPv6.Tags.Add("di.test", "random.tag");
        
        var response = await SynapseClient.Nodes.Add(inetIPv6);
        
        Assert.IsNotNull(response);
        Assert.AreEqual(4, response.Tags.Count());
        Assert.That(response.Tags.Contains("di"));
        Assert.That(response.Tags.Contains("di.test"));
        Assert.That(response.Tags.Contains("random"));
        Assert.That(response.Tags.Contains("random.tag"));
    }
    
    [Test]
    public async Task TestAddCryptoX509CertNode()
    {
        Assert.NotNull(SynapseClient);

        var cert = new CryptoX509Cert();
        cert.MD5 = "ebff56c59290e26d64050e0b68ec6575";
        
        var response = (CryptoX509Cert) await SynapseClient.Nodes.Add(cert);
        
        Assert.IsNotNull(response);
        Assert.AreEqual("ebff56c59290e26d64050e0b68ec6575", response.MD5.ToString());
    }
    
    
    [Test]
    public async Task TestAddTag()
    {
        Assert.NotNull(SynapseClient);

        var inetIPv6 = InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        Console.WriteLine($"'{string.Join(",", inetIPv6.Tags)}'");
        inetIPv6 = await SynapseClient.Nodes.Add(inetIPv6);

        await SynapseClient.Nodes.AddTag(inetIPv6.Iden, "di.test");
        var response = await SynapseClient.Nodes.GetAsync<InetIPv6>(inetIPv6.Iden);
        Console.WriteLine($"'{string.Join(",", response.Tags)}'");
        Assert.That(response.Tags.Contains("di.test"));
    }
    
    [Test]
    public async Task TestDelete()
    {
        Assert.NotNull(SynapseClient);

        var inetIPv6 = InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        inetIPv6 = await SynapseClient.Nodes.Add(inetIPv6);

        var response = SynapseClient.StormAsync<InetIPv6>("inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        Assert.AreEqual(1, await response.CountAsync());

        await SynapseClient.Nodes.Remove(inetIPv6.Iden);
        
        response = SynapseClient.StormAsync<InetIPv6>("inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        Assert.AreEqual(0, await response.CountAsync());
    }
    
    [Test]
    public async Task TestGetByProperty()
    {
        Assert.NotNull(SynapseClient);

        var url = InetUrl.Parse("https://www.welivesecurity.com/2020/03/12/tracking-turla-new-backdoor-armenian-watering-holes/");
        _ = await SynapseClient.Nodes.Add(url);

        var response = SynapseClient.Nodes.GetAsyncByProperty<InetUrl>(new Dictionary<string, string>()
        {
            { "fqdn", "www.welivesecurity.com" }
        });
        Assert.AreEqual(1, await response.CountAsync());
    }
}