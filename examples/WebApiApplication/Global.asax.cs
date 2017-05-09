using System.Web.Http;
using Prometheus.Client.MetricServer;

namespace WebApiApplication
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private IMetricServer _metricServer;

        protected void Application_Start()
        {

            GlobalConfiguration.Configure(WebApiConfig.Register);
            _metricServer = new MetricServer("localhost", 9091);
            _metricServer.Start();
        }

        protected void Application_Stop()
        {
            _metricServer?.Stop();
        }
    }
}
