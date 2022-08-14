using System.Threading.Tasks;
using NUnit.Framework;

namespace Synsharp.Tests;

public class TestView : TestSynapse
{
    [Test]
    public async Task TestListView()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.View);

        var views = await SynapseClient.View.List();
        Assert.AreEqual(1, views.Length);
    }
    
    [Test]
    public async Task TestForkView()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.View);

        var currentView = await SynapseClient.View.GetAsync();
        var countViews = (await SynapseClient.View.List()).Length;
        
        var view = await SynapseClient.View.Fork(name: "my-view");
        Assert.IsNotNull(view);
        Assert.AreEqual("my-view", view.Name);
        Assert.AreEqual(currentView.Iden, view.Parent);
        Assert.AreEqual(countViews + 1, (await SynapseClient.View.List()).Length);
        await SynapseClient.View.Delete(view.Iden);
        Assert.AreEqual(countViews, (await SynapseClient.View.List()).Length);
    }
    
    [Test]
    public async Task TestGetView()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.View);
        var view = await SynapseClient.View.GetAsync();
        Assert.NotNull(view);
    }
}