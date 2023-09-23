namespace Prometheus.Client.MetricServer;

/// <summary>
///     MetricSever
/// </summary>
public interface IMetricServer
{
    /// <summary>
    ///     Server is Running?
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    ///     Start server
    /// </summary>
    void Start();

    /// <summary>
    ///     Stop server
    /// </summary>
    void Stop();
}
