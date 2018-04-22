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
        public async Task CheckUrl()
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