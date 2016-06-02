using WB.Core.BoundedContexts.Headquarters.Views.Interviews;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IChartStatisticsViewFactory
    {
        ChartStatisticsView Load(ChartStatisticsInputModel input);
    }
}