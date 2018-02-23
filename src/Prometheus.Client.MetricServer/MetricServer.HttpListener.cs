#if !NETSTANDARD20 && !NETSTANDARD13

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

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(int port)
            : this(port, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(int port, bool useHttps)
            : this(Consts.DefaultHost, port, Consts.DefaultUrl, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port)
            : this(host, port, false)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, bool useHttps)
            : this(host, port, Consts.DefaultUrl, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, string url, bool useHttps)
            : this(host, port, url, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string hostname, int port, string url, IEnumerable<IOnDemandCollector> standardCollectors, ICollectorRegistry registry, bool useHttps)
            : base(standardCollectors, registry)
        {
            _httpListener.Prefixes.Add($"http{(useHttps ? "s" : "")}://{hostname}:{port}/{url}");
        }

        /// <inheritdoc />
        public void Start()
        {
            if (IsRunning)
                return;
            
            var bgThread = new Thread(StartListen)
            {
                IsBackground = true,
                Name = "MetricServer"
            };
            bgThread.Start();
        }

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public void Stop()
        {
            IsRunning = false;
            _httpListener.Stop();
            _httpListener.Close();
        }

        private void StartListen()
        {
            _httpListener.Start();
            IsRunning = true;

            while (IsRunning)
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
                    Trace.WriteLine($"Error in MetricServer: {ex}");
                }
            }
        }
    }
}

#endif