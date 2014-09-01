using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IChartStatisticsFactory
    {
        ChartStatisticsView Load(ChartStatisticsInputModel input);
    }
}