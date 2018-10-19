#if NET45

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Prometheus.Client.Collectors.Abstractions;

namespace Prometheus.Client.MetricServer
{
    /// <inheritdoc cref="IMetricServer" />
    /// <summary>
    ///     MetricSever based of HttpListener
    /// </summary>
    public class MetricServer : BaseMetricServer, IMetricServer
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly string _mapPath;

        /// <inheritdoc />
        public MetricServer(int port)
            : this(port, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(int port, bool useDefaultCollectors)
            : this(Defaults.Host, port, false, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, bool useHttps)
            : this(host, port, useHttps, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, bool useHttps, bool useDefaultCollectors)
            : this(host, port, Defaults.MapPath, useHttps, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string url, bool useHttps)
            : this(host, port, url, useHttps, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string url, bool useHttps, bool useDefaultCollectors)
            : this(host, port, url, null, new List<IOnDemandCollector>(), useHttps, useDefaultCollectors)
        {
        }
        /// <inheritdoc />
        public MetricServer(string host, int port, string url, ICollectorRegistry registry)
            : this(host, port, url, registry, Defaults.UseDefaultCollectors)
        {
        }
        
        /// <inheritdoc />
        public MetricServer(string host, int port, string url, ICollectorRegistry registry, bool useHttps)
            : this(host, port, url, registry, useHttps, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string url, ICollectorRegistry registry, bool useHttps, bool useDefaultCollectors)
            : this(host, port, url, registry, new List<IOnDemandCollector>(), useHttps, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="mapPath">Map Path: should start with '/'</param>
        /// <param name="registry">Collector registry</param>
        /// <param name="collectors">IOnDemandCollectors</param>
        /// <param name="useHttps">use Https</param>
        /// <param name="useDefaultCollectors">Use default collectors</param>
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry, List<IOnDemandCollector> collectors, bool useHttps,
            bool useDefaultCollectors)
            : base(registry, collectors, useDefaultCollectors)
        {
            if (!mapPath.StartsWith("/"))
                throw new ArgumentException($"mapPath '{mapPath}' should start with '/'");

            _mapPath = mapPath.TrimEnd('/');
            _httpListener.Prefixes.Add($"http{(useHttps ? "s" : "")}://{host}:{port}/");
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

                    if (request.RawUrl.TrimEnd('/') == _mapPath)
                    {
                        response.StatusCode = 200;
                        response.ContentType = Defaults.ContentType;
                        using (var outputStream = response.OutputStream)
                        {
                            ScrapeHandler.Process(Registry, outputStream);
                        }
                    }
                    else
                    {
                        response.StatusCode = 404;
                        using (var outputStream = response.OutputStream)
                        {
                        }
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
