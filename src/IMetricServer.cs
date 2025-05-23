namespace Prometheus.Client.MetricServer;

/// <summary>
/// Interface for the metrics server.
/// </summary>
public interface IMetricServer
{
    /// <summary>
    /// Get a value indicating whether the server is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Start the metrics server.
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the metrics server.
    /// </summary>
    void Stop();
}
