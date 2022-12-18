# Prometheus.Client.MetricServer

[![ci](https://img.shields.io/github/actions/workflow/status/prom-client-net/prom-client-metricserver/ci.yml?branch=main&label=ci&logo=github&style=flat-square)](https://github.com/prom-client-net/prom-client-metricserver/actions/workflows/ci.yml)
[![nuget](https://img.shields.io/nuget/v/Prometheus.Client.MetricServer?logo=nuget&style=flat-square)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![nuget](https://img.shields.io/nuget/dt/Prometheus.Client.MetricServer?logo=nuget&style=flat-square)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![codecov](https://img.shields.io/codecov/c/github/prom-client-net/prom-client-metricserver?logo=codecov&style=flat-square)](https://app.codecov.io/gh/prom-client-net/prom-client-metricserver)
[![codefactor](https://img.shields.io/codefactor/grade/github/prom-client-net/prom-client-metricserver?logo=codefactor&style=flat-square)](https://www.codefactor.io/repository/github/prom-client-net/prom-client-metricserver)
[![license](https://img.shields.io/github/license/prom-client-net/prom-client-metricserver?style=flat-square)](https://github.com/prom-client-net/prom-client-metricserver/blob/main/LICENSE)

Extension for [Prometheus.Client](https://github.com/prom-client-net/prom-client)

## Install

```sh
dotnet add package Prometheus.Client.MetricServer
```

## Use

There are [Examples](https://github.com/prom-client-net/prom-examples)

Simple Console App with static MetricFactory:

```c#
public static void Main(string[] args)
{

    var options = new MetricServerOptions
    {
        Port = 9091
    };

    IMetricServer metricServer = new MetricServer(options);
    metricServer.Start();
    ...

    var counter =  Metrics.DefaultFactory.CreateCounter("test_count", "helptext");
    counter.Inc();
    ...

    metricServer.Stop();
}

```

Worker with DI [extension](https://github.com/prom-client-net/prom-client-dependencyinjection):

```c#
public static async Task Main(string[] args)
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
        {
            services.AddMetricFactory();
            services.AddSingleton<IMetricServer>(sp => new MetricServer(
                new MetricServerOptions
                {
                    CollectorRegistryInstance = sp.GetRequiredService<ICollectorRegistry>(),
                    UseDefaultCollectors = true
                }));
            services.AddHostedService<Worker>();
        }).Build();

    var metricServer = host.Services.GetRequiredService<IMetricServer>();

    try
    {
        metricServer.Start();
        await host.RunAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Host Terminated Unexpectedly");
    }
    finally
    {
        metricServer.Stop();
    }
}

```

## License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).
