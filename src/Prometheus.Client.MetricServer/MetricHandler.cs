using System.Collections.Generic;
using Prometheus.Client.Advanced;

namespace Prometheus.Client.MetricServer
{
    public abstract class MetricHandler 
    {
        protected readonly ICollectorRegistry Registry;

        protected MetricHandler(IEnumerable<IOnDemandCollector> standardCollectors = null,
            ICollectorRegistry registry = null)
        {
            Registry = registry ?? CollectorRegistry.Instance;
            if (Registry != CollectorRegistry.Instance)
                return;

            if (standardCollectors == null)
                standardCollectors = new[] { new DotNetStatsCollector() };

            CollectorRegistry.Instance.RegisterOnDemandCollectors(standardCollectors);
        }

    }
}
