[bmac]: https://www.buymeacoffee.com/phnx47
[ko-fi]: https://ko-fi.com/phnx47
[patreon]: https://www.patreon.com/phnx47

# Prometheus.Client.MetricServer

[![NuGet](https://img.shields.io/nuget/v/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/dt/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![CI](https://github.com/PrometheusClientNet/Prometheus.Client.MetricServer/workflows/CI/badge.svg)](https://github.com/PrometheusClientNet/Prometheus.Client.MetricServer/actions?query=workflow%3ACI)
[![Gitter](https://img.shields.io/gitter/room/PrometheusClientNet/community.svg)](https://gitter.im/PrometheusClientNet/community)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)

Extension for [Prometheus.Client](https://github.com/PrometheusClientNet/Prometheus.Client)

## Install

```sh
dotnet add package Prometheus.Client.MetricServer
```

## Use

There are [Examples](https://github.com/PrometheusClientNet/Prometheus.Client.Examples/tree/master/MetricServer)

Simple Console App with static MetricFactory:

```c#
static void Main(string[] args)
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

## Support

If you like what I'm accomplishing, feel free to buy me a coffee

[<img align="left" alt="phnx47 | Buy Me a Coffe" width="32px" src="https://raw.githubusercontent.com/phnx47/files/master/button-sponsors/bmac0.png" />][bmac]
[<img align="left" alt="phnx47 | Kofi" width="32px" src="https://raw.githubusercontent.com/phnx47/files/master/button-sponsors/kofi0.png" />][ko-fi]
[<img align="left" alt="phnx47 | Patreon" width="32px" src="https://raw.githubusercontent.com/phnx47/files/master/button-sponsors/patreon0.png" />][patreon]

<br />

## License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).
