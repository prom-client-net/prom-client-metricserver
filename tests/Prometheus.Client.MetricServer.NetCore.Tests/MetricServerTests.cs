using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Prometheus.Client.Collectors;
using Xunit;

namespace Prometheus.Client.MetricServer.NetCore.Tests
{
    public class MetricServerTests
    {
        private const int _port = 5050;

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
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
                Assert.False(string.IsNullOrEmpty(response));
            }

            metricServer.Stop();
        }

        [Fact]
        public async Task MapPath_WithEndSlash()
        {
            var metricServer = new MetricServer(
                new CollectorRegistry(),
                new MetricServerOptions { Port = _port, MapPath = "/test" });
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}/test/");
                Assert.False(string.IsNullOrEmpty(response));
            }

            metricServer.Stop();
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
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}" + mapPath);
                Assert.False(string.IsNullOrEmpty(response));
            }

            metricServer.Stop();
        }

        [Fact]
        public async Task FindMetric()
        {
            var registry = new CollectorRegistry();
            var factory = new MetricFactory(registry);
            var metricServer = new MetricServer(registry, new MetricServerOptions { Port = _port });

            metricServer.Start();

            const string metricName = "myCounter";
            var counter = factory.CreateCounter(metricName, "helptext");
            counter.Inc();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{_port}/metrics");
                Assert.Contains(metricName, response);
            }

            metricServer.Stop();
        }

        [Fact]
        public async Task Url_NotFound()
        {
            var metricServer = new MetricServer(new CollectorRegistry(), new MetricServerOptions { Port = _port });
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync($"http://localhost:{_port}");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            metricServer.Stop();
        }
    }
}
