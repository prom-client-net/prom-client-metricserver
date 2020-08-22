# Prometheus.Client.MetricServer

[![NuGet](https://img.shields.io/nuget/v/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![NuGet](https://img.shields.io/nuget/dt/Prometheus.Client.MetricServer.svg)](https://www.nuget.org/packages/Prometheus.Client.MetricServer)
[![CI](https://github.com/PrometheusClientNet/Prometheus.Client.MetricServer/workflows/CI/badge.svg)](https://github.com/PrometheusClientNet/Prometheus.Client.MetricServer/actions?query=workflow%3ACI)
[![Gitter](https://img.shields.io/gitter/room/PrometheusClientNet/community.svg)](https://gitter.im/PrometheusClientNet/community)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)

Extension for [Prometheus.Client](https://github.com/PrometheusClientNet/Prometheus.Client)

#### Installation:

    dotnet add package Prometheus.Client.MetricServer

#### Quick start:

There are [Examples](https://github.com/PrometheusClientNet/Prometheus.Client.Examples/tree/master/MetricServer)

```csharp

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

If you are having problems, send a mail to [prometheus@phnx47.net](mailto://prometheus@phnx47.net). I will try to help you.

## License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).




