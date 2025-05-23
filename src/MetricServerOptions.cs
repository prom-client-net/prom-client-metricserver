using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer;

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
    public int Port { get; set; } = 5000;

    /// <summary>
    ///     Endpoint path
    /// </summary>
    public string MapPath { get; set; } = Defaults.MapPath;

    /// <summary>
    ///     Https certificate
    /// </summary>
    public X509Certificate2 Certificate { get; set; }

    /// <summary>
    ///     Collector Registry instance
    /// </summary>
    public ICollectorRegistry CollectorRegistry { get; set; }

    /// <summary>
    ///     Use default collectors(dotnet and process stats)
    /// </summary>
    public bool UseDefaultCollectors { get; set; } = true;

    /// <summary>
    ///     Charset of text response.
    /// </summary>
    public Encoding ResponseEncoding { get; set; }

    /// <summary>
    ///     Metric prefix for Default collectors
    /// </summary>
    public string MetricPrefixName { get; set; } = string.Empty;
}
