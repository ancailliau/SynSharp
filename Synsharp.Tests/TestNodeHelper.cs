using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;
using Synsharp.Types;
using CryptoX509Cert = Synsharp.Forms.CryptoX509Cert;
using InetIPv6 = Synsharp.Forms.InetIPv6;

namespace Synsharp.Tests;

public class TestNodeHelper : TestSynapse
{   
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
    public async Task TestAddCryptoX509CertNode()
    {
        Assert.NotNull(SynapseClient);

        var cert = new CryptoX509Cert();
        cert.MD5 = "ebff56c59290e26d64050e0b68ec6575";
        
        var response = (CryptoX509Cert) await SynapseClient.Nodes.Add(cert);
        
        Assert.IsNotNull(response);
        Assert.AreEqual("ebff56c59290e26d64050e0b68ec6575", response.MD5.ToString());
    }
}