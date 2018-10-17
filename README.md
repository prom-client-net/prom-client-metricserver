# Prometheus.Client.MetricServer

[![MyGet](https://img.shields.io/myget/phnx47-beta/vpre/Prometheus.Client.MetricServer.svg)](https://www.myget.org/feed/phnx47-beta/package/nuget/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/v/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/dt/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)

[![NuGet Badge](https://buildstats.info/nuget/Prometheus.Client.MetricServer)](https://www.nuget.org/packages/Prometheus.Client.MetricServer/) 
[![Build status](https://ci.appveyor.com/api/projects/status/ea3w0pycgyqqwd1o/branch/master?svg=true)](https://ci.appveyor.com/project/PrometheusClientNet/prometheus-client-metricserver/branch/master)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/12e7517c49aa418b8ae2f242dfb8df2e)](https://www.codacy.com/app/phnx47/Prometheus.Client.MetricServer?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=phnx47/Prometheus.Client.MetricServer&amp;utm_campaign=Badge_Grade) 

Extension for [Prometheus.Client](https://github.com/phnx47/Prometheus.Client)

#### Installation:

    dotnet add package Prometheus.Client.MetricServer

#### Quik start:

There are [Examples](https://github.com/phnx47/Prometheus.Client.Examples/tree/master/MetricServer)

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




