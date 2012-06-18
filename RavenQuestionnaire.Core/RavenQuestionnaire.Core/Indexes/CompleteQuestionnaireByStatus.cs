using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Indexes
{
    public class CompleteQuestionnaireByStatus : AbstractIndexCreationTask<CompleteQuestionnaireStatisticDocument, StatusesWithCompleteQuestionnaireStatistic>
    {
        public CompleteQuestionnaireByStatus()
        {
            Map = cqs => from cq in cqs select new {Id = cq.Status.PublicId, Count = 1};

            Reduce = results => from result in results
                                group result by result.Id
                                into g select new
                                {
                                    Id = g.Key,
                                    Count = g.Sum(x => x.Count)
                                };
            TransformResults = (database, cqs) => from cq in cqs
                                                  //let alias = database.Load<CompleteQuestionnaireStatisticDocument>(cq.Id)
                                                  select new StatusesWithCompleteQuestionnaireStatistic
                                                  {
                                                      Id = cq.Id,
                                                      Count = cq.Count
                                                  };

        }
    }
}
