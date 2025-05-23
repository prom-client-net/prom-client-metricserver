using System.Security.Cryptography.X509Certificates;
using System.Text;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer;

/// <summary>
/// Configuration options for the metrics server.
/// </summary>
public class MetricServerOptions
{
    /// <summary>
    /// The hostname to bind the server to. Default is "*".
    /// </summary>
    public string Host { get; set; } = "*";

    /// <summary>
    /// The port number to listen on. Default is 5000.
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// The endpoint path for metrics. Default is "/metrics".
    /// </summary>
    public string MapPath { get; set; } = Defaults.MapPath;

    /// <summary>
    /// The HTTPS certificate.
    /// </summary>
    public X509Certificate2 Certificate { get; set; }

    /// <summary>
    /// The <see cref="ICollectorRegistry"/> instance to use for metric collection.
    /// </summary>
    public ICollectorRegistry CollectorRegistry { get; set; }

    /// <summary>
    /// Whether to register default collectors. Default is <c>true</c>.
    /// </summary>
    public bool UseDefaultCollectors { get; set; } = true;

    /// <summary>
    /// The text encoding for response content.
    /// </summary>
    public Encoding ResponseEncoding { get; set; }

    /// <summary>
    /// Metric name prefix for default collectors.
    /// </summary>
    public string MetricPrefixName { get; set; } = string.Empty;
}
