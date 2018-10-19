using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Prometheus.Client.MetricServer.NetCore.Tests
{
    public class MetricServerTests
    {
        [Fact]
        public void Start_Stop_IsRunning()
        {
            const int port = 9000;
            var metricServer = new MetricServer(port);
            metricServer.Start();
            Assert.True(metricServer.IsRunning);
            metricServer.Stop();
            Assert.False(metricServer.IsRunning);
        }

        [Fact]
        public async Task Base_MapPath()
        {
            const int port = 9000;
            var metricServer = new MetricServer(port);
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{port}/metrics");
                Assert.False(string.IsNullOrEmpty(response));
            }

            metricServer.Stop();
        }
        
        [Fact]
        public async Task MapPath_WithEndSlash()
        {
            const int port = 9000;
            var metricServer = new MetricServer(port, "/test");
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{port}/test/");
                Assert.False(string.IsNullOrEmpty(response));
            }

            metricServer.Stop();
        }
        
        [Fact]
        public void Wrong_MapPath()
        {
            const int port = 9000;
            Assert.Throws<ArgumentException>(() => new MetricServer(port, "temp"));
        }

        [Theory]
        [InlineData("/metrics")]
        [InlineData("/metrics12")]
        [InlineData("/metrics965")]
        public async Task MapPath(string mapPath)
        {
            const int port = 9000;
            var metricServer = new MetricServer(port, mapPath);
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{port}" + mapPath);
                Assert.False(string.IsNullOrEmpty(response));
            }

            metricServer.Stop();
        }

        [Fact]
        public async Task FindMetric()
        {
            const int port = 9000;
            var metricServer = new MetricServer(port);
            metricServer.Start();

            const string metricName = "myCounter";
            var counter = Metrics.CreateCounter(metricName, "helptext");
            counter.Inc();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"http://localhost:{port}/metrics");
                Assert.Contains(metricName, response);
            }

            metricServer.Stop();
        }

        [Fact]
        public async Task Url_NotFound()
        {
            const int port = 9000;
            var metricServer = new MetricServer(port);
            metricServer.Start();

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync($"http://localhost:{port}");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }

            metricServer.Stop();
        }
    }
}
