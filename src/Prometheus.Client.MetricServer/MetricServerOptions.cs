using System.Security.Cryptography.X509Certificates;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    /// Metric Server Options
    /// </summary>
    public class MetricServerOptions
    {
        /// <summary>
        /// Host name
        /// </summary>
        public string Host { get; set; } = "*";

        /// <summary>
        /// Port number to listen
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Endpoint path
        /// </summary>
        public string MapPath { get; set; } = "/metrics";

#if NETSTANDARD13
        /// <summary>
        /// Https certificate
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
#else
        /// <summary>
        /// Defines if https should be used
        /// </summary>
        public bool UseHttps { get; set; } = false;
#endif
    }
}
