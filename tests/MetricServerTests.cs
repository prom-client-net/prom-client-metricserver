using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Prometheus.Client.Collectors;
using Xunit;

namespace Prometheus.Client.MetricServer.Tests;

public class MetricServerTests(PortFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<PortFixture>
{
    private MetricServer _metricServer = new(new MetricServerOptions
    {
        Port = fixture.Port,
        CollectorRegistry = new CollectorRegistry()
    });

    [Fact]
    public void Null_Options_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MetricServer(null));
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
    public void Start_Stop_WithDefaultPort_IsRunning()
    {
        _metricServer = new MetricServer(new MetricServerOptions { CollectorRegistry = new CollectorRegistry() });
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
    }

    [Fact]
    public void Start_Stop_WithDefaultRegisry_IsRunning()
    {
        _metricServer = new MetricServer();
        _metricServer.Start();
        Assert.True(_metricServer.IsRunning);
        _metricServer.Stop();
        Assert.False(_metricServer.IsRunning);
    }

    [Fact]
    public async Task BaseMapPath_FindMetrics()
    {
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{fixture.Port}{Defaults.MapPath}", TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrEmpty(response));
            Assert.Contains("process_private_memory_bytes", response);
            Assert.Contains("dotnet_total_memory_bytes", response);
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task SetMapPath_FindMetricsWithEndSlash()
    {
        _metricServer = new MetricServer(
            new MetricServerOptions { Port = fixture.Port, CollectorRegistry = new CollectorRegistry(), MapPath = "/test" });
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{fixture.Port}/test/", TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrEmpty(response));
            Assert.Contains("process_private_memory_bytes", response);
            Assert.Contains("dotnet_total_memory_bytes", response);
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Theory]
    [InlineData(Defaults.MapPath)]
    [InlineData("metrics")]
    [InlineData("metrics12")]
    [InlineData("/metrics965")]
    public async Task SetMapPath_FindMetrics(string mapPath)
    {
        _metricServer = new MetricServer(
            new MetricServerOptions { Port = fixture.Port, CollectorRegistry = new CollectorRegistry(), MapPath = mapPath });
        try
        {
            _metricServer.Start();
            using var httpClient = new HttpClient();
            if (!mapPath.StartsWith("/"))
                mapPath = "/" + mapPath;
            string response = await httpClient.GetStringAsync($"http://localhost:{fixture.Port}" + mapPath, TestContext.Current.CancellationToken);
            Assert.False(string.IsNullOrEmpty(response));
            Assert.Contains("process_private_memory_bytes", response);
            Assert.Contains("dotnet_total_memory_bytes", response);
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task CustomCounter_FindMetric()
    {
        var registry = new CollectorRegistry();
        var factory = new MetricFactory(registry);
        _metricServer = new MetricServer(new MetricServerOptions { Port = fixture.Port, CollectorRegistry = registry });

        try
        {
            _metricServer.Start();

            const string metricName = "myCounter";
            var counter = factory.CreateCounter(metricName, "helptext");
            counter.Inc();

            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{fixture.Port}{Defaults.MapPath}", TestContext.Current.CancellationToken);
            Assert.Contains(metricName, response);
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task WrongUrl_NotFound()
    {
        try
        {
            _metricServer.Start();
            var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
            counter.Inc();
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync($"http://localhost:{fixture.Port}/not-found", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }

    [Fact]
    public async Task CustormEncoding_FindHelp()
    {
        var registry = new CollectorRegistry();
        var factory = new MetricFactory(registry);
        _metricServer = new MetricServer(
            new MetricServerOptions { Port = fixture.Port, CollectorRegistry = registry, ResponseEncoding = Encoding.UTF8 });

        try
        {
            _metricServer.Start();

            const string help = "русский хелп";
            var counter = factory.CreateCounter("test_counter_rus", help);
            counter.Inc();

            using var httpClient = new HttpClient();
            string response = await httpClient.GetStringAsync($"http://localhost:{fixture.Port}{Defaults.MapPath}", TestContext.Current.CancellationToken);
            Assert.Contains(help, response);
        }
        catch (Exception ex)
        {
            testOutputHelper.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            _metricServer.Stop();
        }
    }
}
