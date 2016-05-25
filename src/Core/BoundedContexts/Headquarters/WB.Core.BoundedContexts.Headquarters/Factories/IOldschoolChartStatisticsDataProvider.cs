using System;
using WB.Core.BoundedContexts.Headquarters.EventHandler;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IOldschoolChartStatisticsDataProvider
    {
        StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(Guid questionnaireId, long questionnaireVersion);
    }
}