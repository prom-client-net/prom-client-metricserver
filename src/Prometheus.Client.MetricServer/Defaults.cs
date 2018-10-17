namespace Prometheus.Client.MetricServer
{
    internal static class Defaults
    {
        public const string Host = "*";
        public const string MapPath = "/metrics";
        public const bool UseDefaultCollectors = true;
        public const string ContentType = "text/plain; version=0.0.4";
    }
}
