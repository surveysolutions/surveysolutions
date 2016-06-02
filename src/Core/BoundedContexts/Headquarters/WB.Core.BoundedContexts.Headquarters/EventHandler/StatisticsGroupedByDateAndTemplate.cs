using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class StatisticsGroupedByDateAndTemplate : IReadSideRepositoryEntity, IView
    {
        public Dictionary<DateTime, QuestionnaireStatisticsForChart> StatisticsByDate { get; set; }

        public StatisticsGroupedByDateAndTemplate()
        {
            this.StatisticsByDate = new Dictionary<DateTime, QuestionnaireStatisticsForChart>();
        }
    }
}