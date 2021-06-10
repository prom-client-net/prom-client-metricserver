[bmac]: https://www.buymeacoffee.com/phnx47
[ko-fi]: https://ko-fi.com/phnx47
[patreon]: https://www.patreon.com/phnx47

# Prometheus.Client.MetricServer

[![NuGet](https://img.shields.io/nuget/v/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/dt/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![CI](https://img.shields.io/github/workflow/status/prom-client-net/prom-client-metricserver/%F0%9F%92%BF%20CI%20Master?label=CI&logo=github)](https://github.com/prom-client-net/prom-client-metricserver/actions/workflows/master.yml)
[![CodeFactor](https://www.codefactor.io/repository/github/prom-client-net/prom-client-metricserver/badge)](https://www.codefactor.io/repository/github/prom-client-net/prom-client-metricserver)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)

Extension for [Prometheus.Client](https://github.com/prom-client-net/prom-client)

## Install

```sh
dotnet add package Prometheus.Client.MetricServer
```

## Use

There are [Examples](https://github.com/prom-client-net/prom-examples/tree/master/MetricServer)

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

## Support

If you like what I'm accomplishing, feel free to buy me a coffee

[<img align="left" alt="phnx47 | Buy Me a Coffe" width="32px" src="https://raw.githubusercontent.com/phnx47/files/master/button-sponsors/bmac0.png" />][bmac]
[<img align="left" alt="phnx47 | Kofi" width="32px" src="https://raw.githubusercontent.com/phnx47/files/master/button-sponsors/kofi0.png" />][ko-fi]
[<img align="left" alt="phnx47 | Patreon" width="32px" src="https://raw.githubusercontent.com/phnx47/files/master/button-sponsors/patreon0.png" />][patreon]

&nbsp;

## License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).
