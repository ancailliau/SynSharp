using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Synsharp.Tests;

public class TestTag : TestSynapse
{
    [Test]
    public async Task TestGetTag()
    {
        Assert.NotNull(SynapseClient);
        
        // Add a node
        await SynapseClient.StormCallAsync("[ inet:ipv4=8.8.8.8 +#google ]");
        
        // Check for the tag
        var documents = await SynapseClient.StormAsync<InetIpV4>("inet:ipv4=8.8.8.8").ToListAsync();
        Assert.That(documents.First().Tags.Any(t => t == "google"));
    }
}