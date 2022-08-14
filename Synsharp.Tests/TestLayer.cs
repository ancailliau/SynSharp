using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Synsharp.Tests;

public class TestLayer : TestSynapse
{
    [Test]
    public async Task TestListLayer()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.Layer);
        var layers = await SynapseClient.Layer.ListAsync();
        Assert.AreEqual(1, layers.Length);
        Assert.AreEqual("default", layers[0].Name);
    }
    
    [Test]
    public async Task TestAddLayer()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.Layer);
        
        // Create a new layer
        var layer = await SynapseClient.Layer.AddAsync("my-layer");
        Assert.AreEqual("my-layer", layer.Name);
        
        // Get the layers and check that the layer was added
        var layers = await SynapseClient.Layer.ListAsync();
        Assert.AreEqual(2, layers.Length);
        Assert.That(layers.Any(_ => _.Name == "my-layer"));
    }
    
    [Test]
    public async Task TestAddLayerDangerousName()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.Layer);
        
        // Create a new layer
        var layer = await SynapseClient.Layer.AddAsync("\"");
        Assert.AreEqual("\"", layer.Name);
        
        // Get the layers and check that the layer was added
        var layers = await SynapseClient.Layer.ListAsync();
        Assert.AreEqual(2, layers.Length);
        Assert.That(layers.Any(_ => _.Name == "\""));
    }
    
    [Test]
    public async Task TestGetLayer()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.Layer);
        
        // Add a new layer
        var layer = await SynapseClient.Layer.AddAsync("my-layer");
        
        // Retrieve the layer
        var rLayer = await SynapseClient.Layer.GetAsync(layer.Iden);
        Assert.AreEqual("my-layer", rLayer.Name);
    }
}