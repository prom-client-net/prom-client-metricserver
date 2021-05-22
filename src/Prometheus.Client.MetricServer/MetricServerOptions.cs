using System.Security.Cryptography.X509Certificates;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    ///     Metric Server Options
    /// </summary>
    public class MetricServerOptions
    {
        /// <summary>
        ///     Host name
        /// </summary>
        public string Host { get; set; } = "*";

        /// <summary>
        ///     Port number to listen
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///     Endpoint path
        /// </summary>
        public string MapPath { get; set; } = "/metrics";

        /// <summary>
        ///     Https certificate
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
    }
}
