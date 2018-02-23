# Prometheus.Client.MetricServer

[![Build status](https://ci.appveyor.com/api/projects/status/pe2cpegs61b6tmi9?svg=true)](https://ci.appveyor.com/project/phnx47/prometheus-client-metricserver) 
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/12e7517c49aa418b8ae2f242dfb8df2e)](https://www.codacy.com/app/phnx47/Prometheus.Client.MetricServer?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=phnx47/Prometheus.Client.MetricServer&amp;utm_campaign=Badge_Grade) 
[![NuGet Badge](https://buildstats.info/nuget/Prometheus.Client.MetricServer)](https://www.nuget.org/packages/Prometheus.Client.MetricServer/) 

Extension for [Prometheus.Client](https://github.com/phnx47/Prometheus.Client)

## Quik start


#### Install:

    PM> Install-Package Prometheus.Client.MetricServer

#### Use:

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




