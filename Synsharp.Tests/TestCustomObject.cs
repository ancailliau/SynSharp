using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

namespace Synsharp.Tests;

public class TestCustomObject : TestSynapse
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        
        // Add the custom type to synapse
        var command = @"$typeinfo = $lib.dict()
        $forminfo = $lib.dict(doc=""Custom Document Object"")
        $lib.model.ext.addForm(_di:document, str, $typeinfo, $forminfo)";
        await SynapseClient!.StormCallAsync(command);
    }

    [Test]
    public async Task TestCreateCustomObject()
    {
        Assert.NotNull(SynapseClient);
        
        var response = await SynapseClient!
            .StormAsync<SDIDocument>("[ _di:document=\"dil001\" ]")
            .ToListAsync();
        
        Assert.AreEqual(1, response.Count());

        var first = response.Single();
        Assert.That(first.Value.Equals("dil001"));
    }

    [Test]
    public async Task TestCreateCustomObjectLightweightEdge()
    {
        Assert.NotNull(SynapseClient);
        
        _ = await SynapseClient!
            .StormAsync<SynapseObject>("[ _di:document=\"dil001\" inet:ipv4=192.168.3.4 ]")
            .ToListAsync();
        
        _ = await SynapseClient!
            .StormAsync<SDIDocument>("_di:document=\"dil001\" [ <(refs)+ { inet:ipv4=192.168.3.4 } ] ")
            .ToListAsync();
        
        var references = await SynapseClient!
            .StormAsync<InetIPv4>("_di:document=\"dil001\" <(refs)- inet:ipv4 ")
            .ToListAsync();
        foreach (var ipV4 in references)
        {
            Console.WriteLine(ipV4.Value);
        }
        
        var documents = await SynapseClient!
            .StormAsync<SDIDocument>("inet:ipv4=8.8.8.8 -(refs)> _di:document ")
            .ToListAsync();
        foreach (var document in documents)
        {
            Console.WriteLine(document.Value);
        }
    }
}