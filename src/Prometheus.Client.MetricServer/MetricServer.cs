using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer
{
    /// <inheritdoc cref="IMetricServer" />
    /// <summary>
    ///     MetricSever based of Kestrel
    /// </summary>
    public class MetricServer : IMetricServer
    {
        private readonly MetricServerOptions _options;
        private readonly ICollectorRegistry _registry;
        private IWebHost _host;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Http server configuration options</param>
        public MetricServer(MetricServerOptions options)
            :this(null, options)
        {
        }

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

            _registry = registry;
            _options = options;
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
                    if (_options.Certificate != null)
                        options.Listen(IPAddress.Any, _options.Port, listenOptions => { listenOptions.UseHttps(_options.Certificate); });
                })
                .UseUrls($"http{(_options.Certificate != null ? "s" : "")}://{_options.Host}:{_options.Port}")
                .ConfigureServices(services => { services.AddSingleton<IStartup>(new Startup(_registry, _options.MapPath)); })
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
            private const string _contentType = "text/plain; version=0.0.4";

            private ICollectorRegistry _registry;
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
                _registry ??= (ICollectorRegistry)app.ApplicationServices.GetService(typeof(ICollectorRegistry))
                           ?? Metrics.DefaultCollectorRegistry;

                app.Map(_mapPath, coreapp =>
                {
                    coreapp.Run(async context =>
                    {
                        var response = context.Response;
                        response.ContentType = _contentType;

                        using (var outputStream = response.Body)
                        {
                            await ScrapeHandler.ProcessAsync(_registry, outputStream);
                        }
                    });
                });
            }
        }
    }
}
