using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

namespace Synsharp.Tests;

public class TestNodes : TestSynapse
{
    [Test]
    public async Task TestAddIPv6()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetIpV6>("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Equals(IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334")));
    }
    
    [Test]
    public async Task TestGetIPv6()
    {
        Assert.NotNull(SynapseClient);
        
        _ = await SynapseClient
            .StormAsync<InetIpV6>("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]")
            .ToListAsync();
        
        var response = await SynapseClient.StormAsync<InetIpV6>("inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334").ToListAsync();
        Assert.That(response.First().Equals(IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334")));
    }

    [Test]
    public async Task TestAddIPv4()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetIpV4>("[ inet:ipv4=8.8.8.8 ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Equals(IPAddress.Parse("8.8.8.8")));
    }
    
    [Test]
    public async Task TestAddInetUrl()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetUrl>("[ inet:url=\"http://www.example.org/files/index.html\" ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Base.Equals("http://www.example.org/files/index.html"));
        Assert.That(first.FQDN.Equals("www.example.org"));
    }

    [Test]
    public async Task TestAddInetUrlWithUsernameAndPort()
    {
        Assert.NotNull(SynapseClient);

        var url = "http://john.doe:evil@www.example.org:1234/files/index.html?param=evilcorp"; 
        
        await SynapseClient.StormCallAsync<object>($"inet:url=\"{url}\" | delnode");
        
        var response = await SynapseClient
            .StormAsync<InetUrl>($"[ inet:url=\"{url}\" ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.FQDN.Equals("www.example.org"));
        Assert.That(first.User.Equals("john.doe"));
        Assert.That(first.Passwd.Equals("evil"));
        Assert.That(first.Port.Equals(1234));
    }

    [Test]
    public async Task TestAddInetEmail()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetEmail>("[ inet:email=\"john.doe@example.org\" ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.FQDN.Equals("example.org"));
        Assert.That(first.User.Equals("john.doe"));
    }

    [Test]
    public async Task TestAddX509()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<CryptoX509Cert>("[ crypto:x509:cert=* :md5=ebff56c59290e26d64050e0b68ec6575 ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());
        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.AreEqual("ebff56c59290e26d64050e0b68ec6575", first.MD5.Value.ToString());
    }
}