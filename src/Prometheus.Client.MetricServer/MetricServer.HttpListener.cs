#if !COREFX

using System.Collections.Generic;
using System.Net;
using Prometheus.Client.Collectors;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    ///     MetricSever based of HttpListener
    /// </summary>
    public class MetricServer : BaseMetricServer, IMetricServer
    {
        private Thread _bgThread;
        private readonly HttpListener _httpListener = new HttpListener();
        private bool _isListening = false;

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
            _bgThread = new Thread(new ThreadStart(StartListen))
            {
                IsBackground = true,
                Name = "MetricsServer"
            };
            _bgThread.Start();
        }

        private void StartListen()
        {
            _httpListener.Start();
            _isListening = true;

            while (_isListening)
            {
                try
                {
                    HttpListenerContext context = _httpListener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    string requestBody;
                    Stream inputStream = request.InputStream;
                    Encoding encoding = request.ContentEncoding;
                    StreamReader reader = new StreamReader(inputStream, encoding);
                    requestBody = reader.ReadToEnd();
                    response.StatusCode = 200;

                    var acceptHeader = request.Headers.Get("Accept");
                    var acceptHeaders = acceptHeader?.Split(',');
                    var contentType = ScrapeHandler.GetContentType(acceptHeaders);
                    response.ContentType = contentType;

                    using (Stream outputStream = response.OutputStream)
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

        /// <inheritdoc />
        public void Stop()
        {
            _isListening = false;
            _httpListener.Stop();
            _httpListener.Close();

        }
    }
}

#endif