using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IChartStatisticsViewFactory
    {
        ChartStatisticsView Load(ChartStatisticsInputModel input);
    }
}