# Prometheus.Client.MetricServer

[![NuGet Badge](https://buildstats.info/nuget/Prometheus.Client.MetricServer)](https://www.nuget.org/packages/Prometheus.Client.MetricServer/) 
[![Build status](https://ci.appveyor.com/api/projects/status/ea3w0pycgyqqwd1o/branch/master?svg=true)](https://ci.appveyor.com/project/PrometheusClientNet/prometheus-client-metricserver/branch/master)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/12e7517c49aa418b8ae2f242dfb8df2e)](https://www.codacy.com/app/phnx47/Prometheus.Client.MetricServer?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=phnx47/Prometheus.Client.MetricServer&amp;utm_campaign=Badge_Grade) 

Extension for [Prometheus.Client](https://github.com/phnx47/Prometheus.Client)

## Quik start


#### Install:

    dotnet add package Prometheus.Client.MetricServer

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




