using System;
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
    public class MetricServer : IMetricServer
    {
        private const string _contentType = "text/plain; version=0.0.4";

        private readonly ICollectorRegistry _registry;
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly string _mapPath;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="registry">Collector registry </param>
        /// <param name="options">Http server configuration options</param>
        public MetricServer(ICollectorRegistry registry, MetricServerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Port == 0)
                throw new ArgumentException("Port should be specified");

            if (string.IsNullOrEmpty(options.MapPath) || !options.MapPath.StartsWith("/"))
                throw new ArgumentException($"mapPath '{options.MapPath}' should start with '/'");

            _registry = registry ?? Metrics.DefaultCollectorRegistry;

            _mapPath = options.MapPath.EndsWith("/") ? options.MapPath : options.MapPath + "/";
            _httpListener.Prefixes.Add($"http{(options.UseHttps ? "s" : "")}://{options.Host}:{options.Port}/");
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_httpListener.IsListening)
                return;

            _httpListener.Start();
            var bgThread = new Thread(StartListen)
            {
                IsBackground = true,
                Name = "MetricServer"
            };
            bgThread.Start();
        }

        /// <inheritdoc />
        public bool IsRunning => _httpListener.IsListening;

        /// <inheritdoc />
        public void Stop()
        {
            _cancellation.Cancel();
            _httpListener.Stop();
            _httpListener.Close();
        }

        private void StartListen()
        {
            var cancel = _cancellation.Token;

            while (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    var getContext = _httpListener.GetContextAsync();
                    getContext.Wait(cancel);
                    if (cancel.IsCancellationRequested)
                    {
                        return;
                    }

                    var context = getContext.Result;
                    var request = context.Request;
                    var response = context.Response;

                    var rawUrl = request.RawUrl.EndsWith("/") ? request.RawUrl : request.RawUrl + "/";
                    
                    if (rawUrl == _mapPath)
                    {
                        response.StatusCode = 200;
                        response.ContentType = _contentType;
                        using (var outputStream = response.OutputStream)
                        {
                            ScrapeHandler.ProcessAsync(_registry, outputStream).GetAwaiter().GetResult();
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

                catch (Exception ex)
                {
                    Trace.WriteLine($"Error in MetricServer: {ex}");
                }
            }
        }
    }
}
