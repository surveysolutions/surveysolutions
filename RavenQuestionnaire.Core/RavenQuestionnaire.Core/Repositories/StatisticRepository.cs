using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.Statistics;

namespace RavenQuestionnaire.Core.Repositories
{
    public class StatisticRepository :
        EntityRepository<CompleteQuestionnaireStatistics, CompleteQuestionnaireStatisticDocument>, IStatisticRepository
    {
        public StatisticRepository(IDocumentSession documentSession)
            : base(documentSession)
        {
        }

        protected override CompleteQuestionnaireStatistics Create(CompleteQuestionnaireStatisticDocument doc)
        {
            return new CompleteQuestionnaireStatistics(doc);
        }
    }
}
