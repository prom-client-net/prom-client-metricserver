#if !NET45

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors.Abstractions;

#if NETSTANDARD20

using System.Net;

#endif

namespace Prometheus.Client.MetricServer
{
    /// <inheritdoc cref="IMetricServer" />
    /// <summary>
    ///     MetricSever based of Kestrel
    /// </summary>
    public class MetricServer : BaseMetricServer, IMetricServer
    {
        private readonly X509Certificate2 _certificate;
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _mapPath;
        private IWebHost _host;

        /// <inheritdoc />
        public MetricServer(int port)
            : this(port, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(int port, bool useDefaultCollectors)
            : this(Defaults.Host, port, useDefaultCollectors)
        {
        }
        
        /// <inheritdoc />
        public MetricServer(int port, string mapPath)
            : this(port, mapPath, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(int port, string mapPath, bool useDefaultCollectors)
            : this(Defaults.Host, port, mapPath, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port)
            : this(host, port, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, bool useDefaultCollectors)
            : this(host, port, Defaults.MapPath, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath)
            : this(host, port, mapPath, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath, bool useDefaultCollectors)
            : this(host, port, mapPath, null, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(int port, X509Certificate2 certificate)
            : this(port, certificate, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(int port, X509Certificate2 certificate, bool useDefaultCollectors)
            : this(Defaults.Host, port, certificate, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, X509Certificate2 certificate)
            : this(host, port, certificate, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, X509Certificate2 certificate, bool useDefaultCollectors)
            : this(host, port, Defaults.MapPath, null, new List<IOnDemandCollector>(), certificate, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry)
            : this(host, port, mapPath, registry, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry, bool useDefaultCollectors)
            : this(host, port, mapPath, registry, new List<IOnDemandCollector>(), useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry, List<IOnDemandCollector> collectors)
            : this(host, port, mapPath, registry, collectors, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry, List<IOnDemandCollector> collectors, bool useDefaultCollectors)
            : this(host, port, mapPath, registry, collectors, null, useDefaultCollectors)
        {
        }

        /// <inheritdoc />
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry, List<IOnDemandCollector> collectors, X509Certificate2 certificate)
            : this(host, port, mapPath, registry, collectors, certificate, Defaults.UseDefaultCollectors)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="mapPath">Map Path: Should strar with '/'</param>
        /// <param name="registry">Collector registry</param>
        /// <param name="collectors">IOnDemandCollectors</param>
        /// <param name="certificate">Certificate for Https</param>
        /// <param name="useDefaultCollectors">Use default collectors</param>
        public MetricServer(string host, int port, string mapPath, ICollectorRegistry registry, List<IOnDemandCollector> collectors, X509Certificate2 certificate,
            bool useDefaultCollectors)
            : base(registry, collectors, useDefaultCollectors)
        {
            if (!mapPath.StartsWith("/"))
                throw new ArgumentException($"mapPath '{mapPath}' should start with '/'");

            _certificate = certificate;
            _port = port;
            _hostName = host;
            _mapPath = mapPath;
        }

        /// <inheritdoc />
        public bool IsRunning => _host != null;

        /// <inheritdoc />
        public void Start()
        {
            if (IsRunning)
                return;

            var configBuilder = new ConfigurationBuilder();
            configBuilder.Properties["parent"] = this;
            var config = configBuilder.Build();


            _host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel(options =>
                {
#if NETSTANDARD13
                    if (_certificate != null)
                        options.UseHttps(_certificate);
#endif

#if NETSTANDARD20
                    if (_certificate != null)
                        options.Listen(IPAddress.Any, _port, listenOptions => { listenOptions.UseHttps(_certificate); });
#endif
                })
                .UseUrls($"http{(_certificate != null ? "s" : "")}://{_hostName}:{_port}")
                .ConfigureServices(services => { services.AddSingleton<IStartup>(new Startup(Registry, _mapPath)); })
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(Startup).GetTypeInfo().Assembly.FullName)
                .Build();

            _host.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (!IsRunning)
                return;

            _host.Dispose();
            _host = null;
        }

        internal class Startup : IStartup
        {
            private readonly ICollectorRegistry _registry;
            private readonly string _mapPath;

            public Startup(ICollectorRegistry registry, string mapPath)
            {
                _registry = registry;
                _mapPath = mapPath;

                var builder = new ConfigurationBuilder();
                Configuration = builder.Build();
            }

            public IConfigurationRoot Configuration { get; }

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                return services.BuildServiceProvider();
            }

            public void Configure(IApplicationBuilder app)
            {
                app.Map(_mapPath, coreapp =>
                {
                    coreapp.Run(async context =>
                    {
                        var response = context.Response;
                        response.ContentType =  Defaults.ContentType;

                        using (var outputStream = response.Body)
                        {
                            ScrapeHandler.Process(_registry, outputStream);
                        }

                        await Task.FromResult(0).ConfigureAwait(false);
                    });
                });
            }
        }
    }
}

#endif
