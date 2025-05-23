using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer;

/// <summary>
/// Kestrel-based implementation of the metrics server.
/// </summary>
public class MetricServer : IMetricServer
{
    private readonly MetricServerOptions _options;
    private IWebHost _host;

    /// <summary>
    /// Initialize a new instance of the <see cref="MetricServer"/> class with default options.
    /// </summary>
    public MetricServer()
        : this(new MetricServerOptions())
    {
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="MetricServer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The <see cref="MetricServerOptions"/> configuration.</param>
    public MetricServer(MetricServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.MapPath.StartsWith('/'))
            options.MapPath = "/" + options.MapPath;
        _options = options;
        _options.CollectorRegistry ??= Metrics.DefaultCollectorRegistry;
        if (_options.UseDefaultCollectors)
            options.CollectorRegistry.UseDefaultCollectors(options.MetricPrefixName);
    }

    public bool IsRunning => _host != null;

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

    public void Stop()
    {
        if (!IsRunning)
            return;
        _host.Dispose();
        _host = null;
    }

    internal class Startup(MetricServerOptions options) : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            var contentType = options.ResponseEncoding != null
                ? $"{Defaults.ContentType}; charset={options.ResponseEncoding.BodyName}"
                : Defaults.ContentType;
            app.Map(options.MapPath, coreapp =>
            {
                coreapp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = contentType;
                    await using var outputStream = response.Body;
                    await ScrapeHandler.ProcessAsync(options.CollectorRegistry, outputStream);
                });
            });
        }
    }
}
