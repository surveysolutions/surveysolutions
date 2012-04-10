using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.Statistics;

namespace RavenQuestionnaire.Core.Repositories
{
    public interface IStatisticRepository : IEntityRepository<CompleteQuestionnaireStatistics, CompleteQuestionnaireStatisticDocument>
    {
    }
}
