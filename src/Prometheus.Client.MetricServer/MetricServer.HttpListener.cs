#if !COREFX

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    ///     MetricSever based of HttpListener
    /// </summary>
    public class MetricServer : BaseMetricServer, IMetricServer
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private Thread _bgThread;
        private bool _isListening;

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
            _bgThread = new Thread(StartListen)
            {
                IsBackground = true,
                Name = "MetricsServer"
            };
            _bgThread.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _isListening = false;
            _httpListener.Stop();
            _httpListener.Close();
        }

        private void StartListen()
        {
            _httpListener.Start();
            _isListening = true;

            while (_isListening)
            {
                try
                {
                    var context = _httpListener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    var inputStream = request.InputStream;
                    var encoding = request.ContentEncoding;
                    var reader = new StreamReader(inputStream, encoding);
                    reader.ReadToEnd();
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
                }

                catch (HttpListenerException ex)
                {
                    Trace.WriteLine($"Error in MetricsServer: {ex}");
                }
            }
        }
    }
}

#endif