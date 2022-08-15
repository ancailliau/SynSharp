using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Synsharp.Forms;

namespace Synsharp.Tests;

[TestFixture]
public class TestEquality
{
    
    [Test]
    public async Task TestSameIPv6AreEquals()
    {
        var a = InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        var b = InetIPv6.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        Assert.That(a.Equals(b));
    }
    
    [Test]
    public async Task TestSameEmailAreEquals()
    {
        var a = new InetEmail("bob@example.org");
        var b = new InetEmail("bob@example.org");
        Assert.That(a.Equals(b));
    }
    
    [Test]
    public async Task TestSameUrlAreEquals()
    {
        var a = new InetUrl("https://twitter.com/home");
        var b = new InetUrl("https://twitter.com/home");
        Assert.That(a.Equals(b));
    }
    
    [Test]
    public async Task TestSameStrAreEquals()
    {
        var a = Synsharp.Types.Str.Parse("a");
        var b = Synsharp.Types.Str.Parse("a");
        Assert.That(a.Equals(b));
    }
}