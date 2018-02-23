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
        ///     Standard collectors
        /// </summary>
        private readonly IEnumerable<IOnDemandCollector> _standardCollectors = new[] { new DotNetStatsCollector() };

        
        /// <summary>
        ///     Constructor
        /// </summary>
        protected BaseMetricServer()
            :this(null, null)
        {
           
        }
        
        /// <summary>
        ///     Constructor
        /// </summary>
        protected BaseMetricServer(IEnumerable<IOnDemandCollector> standardCollectors)
            :this(standardCollectors, null)
        {
           
        }
        
        /// <summary>
        ///     Constructor
        /// </summary>
        protected BaseMetricServer(ICollectorRegistry registry)
            :this(null, registry)
        {
           
        }
        
        /// <summary>
        ///     Constructor
        /// </summary>
        protected BaseMetricServer(IEnumerable<IOnDemandCollector> standardCollectors, ICollectorRegistry registry)
        {
            Registry = registry ?? CollectorRegistry.Instance;
            
            if (Registry != CollectorRegistry.Instance)
                return;

            if (standardCollectors != null)
                _standardCollectors = standardCollectors;

            CollectorRegistry.Instance.RegisterOnDemandCollectors(_standardCollectors);
        }
    }
}