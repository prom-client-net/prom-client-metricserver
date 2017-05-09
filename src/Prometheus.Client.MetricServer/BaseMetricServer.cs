using System.Collections.Generic;
using Prometheus.Client.Collectors;

namespace Prometheus.Client.MetricServer
{
    /// <summary>
    ///     Base Abstract MetricSever
    /// </summary>
    public abstract class BaseMetricServer
    {
        /// <summary>
        ///     CollectorRegistry
        /// </summary>
        protected readonly ICollectorRegistry Registry;

        /// <summary>
        ///     Constructor
        /// </summary>
        protected BaseMetricServer(IEnumerable<IOnDemandCollector> standardCollectors = null,
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