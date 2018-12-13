using System;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IOldschoolChartStatisticsDataProvider
    {
        StatisticsGroupedByDateAndTemplate GetStatisticsInOldFormat(QuestionnaireIdentity questionnaireIdentity);
    }
}
