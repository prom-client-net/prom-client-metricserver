using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Prometheus.Client.Collectors;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.Client.MetricServer.Tests;

public class MetricServerTests
{
    private IMetricServer _metricServer;
    private readonly ITestOutputHelper _testOutputHelper;
    private const int _port = 9091;

    public MetricServerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _metricServer = new MetricServer(new MetricServerOptions
        {
            Port = _port,
            CollectorRegistryInstance = new CollectorRegistry()
        });
    }

    [Fact]
    public void Start_Stop_IsRunning()
    {
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
    }

    [Fact]
    public void Start_DoubleStop_IsRunning()
    {
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
    }

    [Fact]
    public void DoubleStart_Stop_IsRunning()
    {
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
    }

    [Fact]
    public void Start_Stop_DefaultPort_IsRunning()
    {
        _metricServer = new MetricServer(new MetricServerOptions { CollectorRegistryInstance = new CollectorRegistry() });
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
    }

    [Fact]
    public async Task Base_MapPath()
    {
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
            Assert.False(string.IsNullOrEmpty(response));
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task MapPath_WithEndSlash()
    {
        _metricServer = new MetricServer(new MetricServerOptions { Port = _port, CollectorRegistryInstance = new CollectorRegistry(), MapPath = "/test" });
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/test/");
            Assert.False(string.IsNullOrEmpty(response));
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public void Wrong_MapPath()
    {
        Assert.Throws<ArgumentException>(() => new MetricServer(
            new MetricServerOptions { Port = _port, MapPath = "temp" }));
    }

    [Theory]
    [InlineData("/metrics")]
    [InlineData("/metrics12")]
    [InlineData("/metrics965")]
    public async Task MapPath(string mapPath)
    {
        _metricServer= new MetricServer(new MetricServerOptions { Port = _port, CollectorRegistryInstance = new CollectorRegistry(), MapPath = mapPath });
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}" + mapPath);
            Assert.False(string.IsNullOrEmpty(response));
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task Custom_Find_Metric()
    {
        var registry = new CollectorRegistry();
        var factory = new MetricFactory(registry);
        _metricServer = new MetricServer(new MetricServerOptions { Port = _port, CollectorRegistryInstance = registry });

        try
        {
            _metricServer.Start();

            const string metricName = "myCounter";
            var counter = factory.CreateCounter(metricName, "helptext");
            counter.Inc();

            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
            Assert.Contains(metricName, response);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task AddLegacyMetrics_False_CheckMetrics()
    {
        try
        {
            _metricServer.Start();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
            Assert.Contains("process_private_memory_bytes", response);
            Assert.Contains("dotnet_total_memory_bytes", response);
            Assert.DoesNotContain("process_private_bytes", response);
            Assert.DoesNotContain("dotnet_totalmemory", response);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task AddLegacyMetrics_True_CheckMetrics()
    {
        _metricServer = new MetricServer(new MetricServerOptions { Port = _port, CollectorRegistryInstance = new CollectorRegistry(), AddLegacyMetrics = true });

        try
        {
            _metricServer.Start();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
            Assert.Contains("process_private_memory_bytes", response);
            Assert.Contains("dotnet_total_memory_bytes", response);
            Assert.Contains("process_private_bytes", response);
            Assert.Contains("dotnet_totalmemory", response);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task Url_NotFound()
    {
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"http://localhost:{_port}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task Find_Default_Metric()
    {
        _metricServer = new MetricServer(new MetricServerOptions { Port = _port, CollectorRegistryInstance = new CollectorRegistry(), UseDefaultCollectors = true });

        try
        {
            _metricServer.Start();

            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
            Assert.Contains("dotnet_collection_count_total", response);
            Assert.Contains("process_cpu_seconds_total", response);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task Add_Encoding()
    {
        var registry = new CollectorRegistry();
        var factory = new MetricFactory(registry);
        _metricServer = new MetricServer(new MetricServerOptions { Port = _port, CollectorRegistryInstance = registry, ResponseEncoding = Encoding.UTF8 });

        try
        {
            _metricServer.Start();

            const string help = "русский хелп";
            var counter = factory.CreateCounter("test_counter_rus", help);
            counter.Inc();

            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
            Assert.Contains(help, response);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }


    [Fact]
    public void Null_Options_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MetricServer(null));
    }
}
