using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Synsharp.Tests;

public abstract class TestSynapse
{
    protected SynapseClient? SynapseClient;
    private TestcontainersContainer? _testContainers;

    [SetUp]
    public virtual async Task Setup()
    {
        var builder = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("vertexproject/synapse-cortex:v2.x.x")
            .WithName("synpase")
            .WithPortBinding(8901, 4443)
            .WithEnvironment("SYN_CORTEX_AUTH_PASSWD", "secret")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4443));
        
        _testContainers = builder.Build();
        await _testContainers.StartAsync();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddConsole(options => options.DisableColors = true);
        });
        
        SynapseClient = new SynapseClient("https://localhost:8901", loggerFactory);
        await SynapseClient.LoginAsync("root", "secret");
    }

    [TearDown]
    public async Task Teardown()
    {
        if (_testContainers != null)
        {
            await _testContainers.StopAsync();
            await _testContainers.DisposeAsync();
        }
    }
}