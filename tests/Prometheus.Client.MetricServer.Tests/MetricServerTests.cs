using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Prometheus.Client.Collectors;
using Xunit;
using Xunit.Abstractions;

namespace Prometheus.Client.MetricServer.Tests
{
    public class MetricServerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const int _port = 5055;

        public MetricServerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Start_Stop_IsRunning()
        {
            var metricServer = new MetricServer(new CollectorRegistry(), new MetricServerOptions { Port = _port });
            metricServer.Start();
            Assert.True(metricServer.IsRunning);
            metricServer.Stop();
            Assert.False(metricServer.IsRunning);
        }

        [Fact]
        public async Task Base_MapPath()
        {
            var metricServer = new MetricServer(new CollectorRegistry(), new MetricServerOptions { Port = _port });
            try
            {
                metricServer.Start();
                var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
                counter.Inc();
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
                Assert.False(string.IsNullOrEmpty(response));
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                metricServer.Stop();
            }
        }

        [Fact]
        public async Task MapPath_WithEndSlash()
        {
            var metricServer = new MetricServer(
                new CollectorRegistry(),
                new MetricServerOptions { Port = _port, MapPath = "/test" });
            try
            {
                metricServer.Start();
                var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
                counter.Inc();
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}/test/");
                Assert.False(string.IsNullOrEmpty(response));
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                metricServer.Stop();
            }
        }

        [Fact]
        public void Wrong_MapPath()
        {
            Assert.Throws<ArgumentException>(() => new MetricServer(
                new CollectorRegistry(),
                new MetricServerOptions { Port = _port, MapPath = "temp" }));
        }

        [Theory]
        [InlineData("/metrics")]
        [InlineData("/metrics12")]
        [InlineData("/metrics965")]
        public async Task MapPath(string mapPath)
        {
            var metricServer = new MetricServer(
                new CollectorRegistry(),
                new MetricServerOptions { Port = _port, MapPath = mapPath });
            try
            {
                metricServer.Start();
                var counter = Metrics.DefaultFactory.CreateCounter("test_counter", "help");
                counter.Inc();
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}" + mapPath);
                Assert.False(string.IsNullOrEmpty(response));
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                metricServer.Stop();
            }
        }

        [Fact]
        public async Task FindMetric()
        {
            var registry = new CollectorRegistry();
            var factory = new MetricFactory(new CollectorRegistry());
            var metricServer = new MetricServer(registry, new MetricServerOptions { Port = _port });

            try
            {
                metricServer.Start();

                const string metricName = "myCounter";
                var counter = factory.CreateCounter(metricName, "helptext");
                counter.Inc();

                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
                Assert.Contains(metricName, response);
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                metricServer.Stop();
            }
        }

        [Fact]
        public async Task Url_NotFound()
        {
            var metricServer = new MetricServer(new CollectorRegistry(), new MetricServerOptions { Port = _port });
            try
            {
                metricServer.Start();
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
                metricServer.Stop();
            }
        }
    }
}
