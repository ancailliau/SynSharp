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
            .StormAsync<InetIPv6>("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Equals(InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334")));
    }
    
    [Test]
    public async Task TestGetIPv6()
    {
        Assert.NotNull(SynapseClient);
        
        _ = await SynapseClient
            .StormAsync<InetIPv6>("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]")
            .ToListAsync();
        
        var response = await SynapseClient.StormAsync<InetIPv6>("inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334").ToListAsync();
        Assert.That(response.First().Equals(InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334")));
    }

    [Test]
    public async Task TestAddIPv4()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient
            .StormAsync<InetIPv4>("[ inet:ipv4=8.8.8.8 ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.IsNotNull(first);
        Assert.That(first.Equals(InetIPv4.Parse("8.8.8.8")));
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
        Assert.That(first.Base.Equals(Types.Str.Parse("http://www.example.org/files/index.html")), $"Expected 'http://www.example.org/files/index.html' but got '{first.Base}' "); 
        Assert.That(first.FQDN.Equals(Types.InetFqdn.Parse("www.example.org")), $"Expected 'www.example.org' but got '{first.FQDN}' ");
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
        Assert.That(first.FQDN.Equals(Types.InetFqdn.Convert("www.example.org")), $"Expected 'www.example.org' but got '{first.FQDN}' ");
        Assert.That(first.User.Equals(Types.InetUser.Convert("john.doe")), $"Expected 'john.doe' but got '{first.User}' ");
        Assert.That(first.Passwd.Equals(Types.InetPasswd.Convert("evil")), $"Expected 'evil' but got '{first.Passwd}' ");
        Assert.That(first.Port.Equals(Types.InetPort.Convert(1234)), $"Expected '1234' but got '{first.Port}' ");
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
        Assert.That(first.FQDN.Equals(Types.InetFqdn.Convert("example.org")), $"Expected 'example.org' but got '{first.FQDN}' ");
        Assert.That(first.User.Equals(Types.InetUser.Convert("john.doe")), $"Expected 'john.doe' but got '{first.User}' ");
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
        Assert.AreEqual("ebff56c59290e26d64050e0b68ec6575", first.MD5.ToString());
    }
}