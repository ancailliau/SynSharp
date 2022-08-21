using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

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
    
    [Test]
    public async Task TestExecuteView()
    {
        Assert.NotNull(SynapseClient);
        Assert.NotNull(SynapseClient.View);
        var view = await SynapseClient.View.Fork(name: "my-view");
        var view2 = await SynapseClient.View.Fork(name: "my-view-2");
        Assert.NotNull(view);
        Assert.NotNull(view2);

        var results = await (SynapseClient.View.Execute<SynapseObject>(view.Iden, "[ inet:ipv4=8.8.8.8 ]")).ToListAsync();
        Assert.AreEqual(1, results.Count);
        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
        
        var result1 = SynapseClient.View.Execute<InetIPv4>(view.Iden, "inet:ipv4");
        Assert.AreEqual(1, await result1.CountAsync());
        var result2 = SynapseClient.View.Execute<InetIPv4>(view2.Iden, "inet:ipv4");
        Assert.AreEqual(0, await result2.CountAsync());
        var result3 = SynapseClient.StormAsync<InetIPv4>("inet:ipv4");
        Assert.AreEqual(0, await result3.CountAsync());

        /*
        _ = SynapseClient.View.Merge(view.Iden);
        var result4 = SynapseClient.StormAsync<InetIPv4>("inet:ipv4");
        Assert.Equals(1, result4);
        */
    }
}