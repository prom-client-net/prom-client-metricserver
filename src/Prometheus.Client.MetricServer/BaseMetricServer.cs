using System.Collections.Generic;
using Prometheus.Client.Collectors;
using Prometheus.Client.Collectors.Abstractions;

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
        /// <param name="registry">Collector registry</param>
        /// <param name="collectors">IOnDemandCollectors</param>
        /// <param name="useDefaultCollectors">Use default collectors</param>
        protected BaseMetricServer(ICollectorRegistry registry, List<IOnDemandCollector> collectors, bool useDefaultCollectors)
        {
            Registry = registry ?? CollectorRegistry.Instance;
            if (useDefaultCollectors)
            {
                var metricFactory = Registry == CollectorRegistry.Instance
                    ? Metrics.DefaultFactory
                    : new MetricFactory(Registry);

                collectors.AddRange(DefaultCollectors.Get(metricFactory));
            }

            Registry.RegisterOnDemandCollectors(collectors);
        }
    }
}
