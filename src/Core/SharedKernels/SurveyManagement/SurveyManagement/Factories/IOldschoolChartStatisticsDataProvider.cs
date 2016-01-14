using System;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IOldschoolChartStatisticsDataProvider
    {
        StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(Guid questionnaireId, long questionnaireVersion);
    }
}