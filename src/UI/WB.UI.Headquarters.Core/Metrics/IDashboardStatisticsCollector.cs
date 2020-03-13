using System.Collections.Generic;

namespace WB.UI.Headquarters.Metrics
{
    public interface IDashboardStatisticsService
    {
        List<MetricState> GetState();        
    }
}
