#if !COREFX

using System.Collections.Generic;
using System.Net;
using Prometheus.Client.Advanced;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    ///     MetricSever based of HttpListener
    /// </summary>
    public class MetricServer : BaseMetricServer, IMetricServer
    {
        private readonly HttpListener _httpListener = new HttpListener();

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(int port, bool useHttps = false)
            : this("+", port, Consts.DefaultUrl, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, bool useHttps = false)
            : this(host, port, Consts.DefaultUrl, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, string url, bool useHttps = false)
            : this(host, port, url, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string hostname, int port, string url, IEnumerable<IOnDemandCollector> standardCollectors = null, ICollectorRegistry registry = null, bool useHttps = false)
            : base(standardCollectors, registry)
        {
            var s = useHttps ? "s" : "";
            _httpListener.Prefixes.Add($"http{s}://{hostname}:{port}/{url}");
        }

        /// <inheritdoc />
        public void Start()
        {
            _httpListener.Start();
            _httpListener.BeginGetContext(ar =>
            {
                var httpListenerContext = _httpListener.EndGetContext(ar);
                var request = httpListenerContext.Request;
                var response = httpListenerContext.Response;

                response.StatusCode = 200;

                var acceptHeader = request.Headers.Get("Accept");
                var acceptHeaders = acceptHeader?.Split(',');
                var contentType = ScrapeHandler.GetContentType(acceptHeaders);
                response.ContentType = contentType;

                using (var outputStream = response.OutputStream)
                {
                    var collected = Registry.CollectAll();
                    ScrapeHandler.ProcessScrapeRequest(collected, contentType, outputStream);
                }

                response.Close();
            }, null);
        }

        /// <inheritdoc />
        public void Stop()
        {
            _httpListener.Stop();
            _httpListener.Close();
        }
    }
}

#endif