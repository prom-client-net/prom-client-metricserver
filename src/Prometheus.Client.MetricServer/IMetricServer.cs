namespace Prometheus.Client.MetricServer
{
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
    }
}