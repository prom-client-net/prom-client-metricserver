namespace Prometheus.Client.MetricServer.Kestrel
{
    /// <summary>
    ///     MetricSever
    /// </summary>
    public interface IMetricServer
    {
        /// <summary>
        ///     Start server
        /// </summary>
        void Start();

        /// <summary>
        ///     Stop server
        /// </summary>
        void Stop();
        
        /// <summary>
        ///     Server is Running?
        /// </summary>
        bool IsRunning { get; }
    }
}
