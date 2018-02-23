#if NETSTANDARD13 || NETSTANDARD20

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    ///     MetricSever based of Kestrel
    /// </summary>
    public class MetricServer : BaseMetricServer, IMetricServer
    {
        private readonly X509Certificate2 _certificate;
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _url;
        private readonly bool _useHttps;
        private IWebHost _host;


        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(int port, bool useHttps = false)
            : this(Consts.DefaultHost, port, Consts.DefaultUrl, null, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, bool useHttps = false)
            : this(host, port, Consts.DefaultUrl, null, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, string url, bool useHttps = false)
            : this(host, port, url, null, null, null, useHttps)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MetricServer(string host, int port, string url, IEnumerable<IOnDemandCollector> standardCollectors = null, ICollectorRegistry registry = null,
            X509Certificate2 certificate = null, bool useHttps = false)
            : base(standardCollectors, registry)
        {
            if (useHttps && certificate == null)
                throw new ArgumentNullException(nameof(certificate), $"{nameof(certificate)} is required when using https");

            _useHttps = useHttps;
            _certificate = certificate;
            _port = port;
            _hostName = host;
            _url = url;
        }

        /// <summary>
        ///     Server is Running?
        /// </summary>
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
                    if (_useHttps)
                        options.UseHttps(_certificate);
#endif

#if NETSTANDARD20
                    if (_useHttps)
                        options.Listen(IPAddress.Any, _port, listenOptions => { listenOptions.UseHttps(_certificate); });
#endif
                })
                .UseUrls($"http{(_useHttps ? "s" : "")}://{_hostName}:{_port}")
                .ConfigureServices(services => { services.AddSingleton<IStartup>(new Startup(Registry, _url)); })
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
            private readonly string _url;

            public Startup(ICollectorRegistry registry, string url)
            {
                _registry = registry;
                _url = url;

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
                app.Run(context =>
                {
                    if (context.Request.Path == _url)
                    {
                        var response = context.Response;
                        var request = context.Request;
                        response.StatusCode = 200;

                        var acceptHeader = request.Headers["Accept"];
                        var contentType = ScrapeHandler.GetContentType(acceptHeader);
                        response.ContentType = contentType;

                        using (var outputStream = response.Body)
                        {
                            var collected = _registry.CollectAll();
                            ScrapeHandler.ProcessScrapeRequest(collected, contentType, outputStream);
                        }

                        return Task.FromResult(true);
                    }

                    context.Response.StatusCode = 404;
                    return Task.FromResult(true);
                });
            }
        }
    }
}

#endif