# Prometheus.Client.MetricServer

[![Build status](https://ci.appveyor.com/api/projects/status/pe2cpegs61b6tmi9?svg=true)](https://ci.appveyor.com/project/phnx47/prometheus-client-metricserver) [![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT) [![NuGet Badge](https://buildstats.info/nuget/Prometheus.Client.MetricServer)](https://www.nuget.org/packages/Prometheus.Client.MetricServer/) 

Extension for [Prometheus.Client](https://github.com/phnx47/Prometheus.Client)

## Quik start


#### Install:

    PM> Install-Package Prometheus.Client.MetricServer

#### Use:

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


