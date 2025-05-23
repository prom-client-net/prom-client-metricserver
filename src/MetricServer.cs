using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer;

/// <inheritdoc cref="IMetricServer" />
/// <summary>
///     MetricSever based of Kestrel
/// </summary>
public class MetricServer : IMetricServer
{
    private readonly MetricServerOptions _options;
    private IWebHost _host;

    /// <summary>
    ///     Constructor
    /// </summary>
    public MetricServer()
        : this(new MetricServerOptions())
    {
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    public MetricServer(MetricServerOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (!options.MapPath.StartsWith("/"))
            options.MapPath = "/" + options.MapPath;

        _options = options;

        _options.CollectorRegistry ??= Metrics.DefaultCollectorRegistry;

        if (_options.UseDefaultCollectors)
        {
#pragma warning disable CS0618
            if (options.AddLegacyMetrics)
                options.CollectorRegistry.UseDefaultCollectors(options.MetricPrefixName, options.AddLegacyMetrics);
            else
                options.CollectorRegistry.UseDefaultCollectors(options.MetricPrefixName);
#pragma warning restore CS0618
        }
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
            .ConfigureServices(services => { services.AddSingleton<IStartup>(new Startup(_options)); })
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
        private readonly MetricServerOptions _options;

        public Startup(MetricServerOptions options)
        {
            _options = options;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            var contentType = _options.ResponseEncoding != null
                ? $"{Defaults.ContentType}; charset={_options.ResponseEncoding.BodyName}"
                : Defaults.ContentType;

            app.Map(_options.MapPath, coreapp =>
            {
                coreapp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = contentType;

                    await using var outputStream = response.Body;
                    await ScrapeHandler.ProcessAsync(_options.CollectorRegistry, outputStream);
                });
            });
        }
    }
}
