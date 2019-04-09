# Prometheus.Client.MetricServer

[![MyGet](https://img.shields.io/myget/prometheus-client-net/vpre/Prometheus.Client.MetricServer.svg?label=myget)](https://www.myget.org/feed/prometheus-client-net/package/nuget/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/v/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/dt/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![Gitter](https://img.shields.io/gitter/room/PrometheusClientNet/community.svg)](https://gitter.im/PrometheusClientNet/community)

[![Build status](https://ci.appveyor.com/api/projects/status/ea3w0pycgyqqwd1o/branch/master?svg=true)](https://ci.appveyor.com/project/PrometheusClientNet/prometheus-client-metricserver/branch/master)
[![AppVeyor tests](https://img.shields.io/appveyor/tests/PrometheusClientNet/prometheus-client-metricserver.svg)](https://ci.appveyor.com/project/PrometheusClientNet/prometheus-client-metricserver/build/tests)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)

Extension for [Prometheus.Client](https://github.com/PrometheusClientNet/Prometheus.Client)

#### Installation:

    dotnet add package Prometheus.Client.MetricServer

#### Quick start:

There are [Examples](https://github.com/PrometheusClientNet/Prometheus.Client.Examples/tree/master/MetricServer)

```csharp

static void Main(string[] args)
{
    IMetricServer metricServer = new MetricServer("localhost", 9091);
    metricServer.Start();
    ...
    
    var counter = Metrics.CreateCounter("test_count", "helptext");
    counter.Inc();
    ...     
    
    metricServer.Stop();
}
```

## Support

If you are having problems, send a mail to [prometheus@phnx47.net](mailto://prometheus@phnx47.net). I will try to help you.

I would also very much appreciate your support by buying me a coffee.

<a href="https://www.buymeacoffee.com/phnx47" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).




