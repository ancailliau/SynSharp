using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;
using Synsharp.Telepath;
using Synsharp.Telepath.Messages;

namespace Synsharp.Tests.Telepath;

public class TestLongTermBugTelepath : TestTelepath
{
    [Test]
    public async Task TestCrashWhenManyRequest()
    {
        Assert.NotNull(SynapseClient);

        var proxy = await SynapseClient.GetProxyAsync();
        for (int i = 0; i < MAX; i++)
        {
            var response = await proxy.Storm("[ inet:ipv6=2001:0db8:85a3:0000:0000:8a2e:0370:7334 ]", new StormOps() {Repr = true})
                .OfType<SynapseNode>()
                .ToListAsync();

            var first = response.Single();
            Assert.IsNotNull(first);
            Assert.That(response.First().Valu.Equals("2001:db8:85a3::8a2e:370:7334"));
            if (i % 1000 == 0) Console.WriteLine(i);
        }
    }

    private const int MAX = 10;
}