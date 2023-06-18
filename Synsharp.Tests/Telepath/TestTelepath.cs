using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Synsharp.Forms;
using Synsharp.Telepath;

namespace Synsharp.Tests;

public class TestTelepath
{
    protected TelepathClient? SynapseClient;
    private TestcontainersContainer? _testContainers;
    private ILoggerFactory _loggerFactory;

    [SetUp]
    public virtual async Task Setup()
    {
        var builder = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("vertexproject/synapse-cortex:v2.x.x")
            .WithName("synpase")
            .WithPortBinding(8903, 27492)
            .WithEnvironment("SYN_CORTEX_AUTH_PASSWD", "secret")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4443));
        
        _testContainers = builder.Build();
        await _testContainers.StartAsync();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddConsole(options => options.DisableColors = true);
        });

        var proxyOptions = new ProxyOptions()
        {
            
        };
        var clientConfiguration = new ClientConfiguration()
        {
            
        };
        SynapseClient = new TelepathClient("tcp://root:secret@localhost:8903/", proxyOptions, clientConfiguration, _loggerFactory);
    }

    [TearDown]
    public async Task Teardown()
    {
        if (_testContainers != null)
        {
            await _testContainers.StopAsync();
            await _testContainers.DisposeAsync();
            _loggerFactory.Dispose();
        }
    }
}