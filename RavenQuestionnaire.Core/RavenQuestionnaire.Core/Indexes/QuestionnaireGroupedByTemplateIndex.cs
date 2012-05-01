using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.Indexes
{
    /*public class QuestionnaireGroupedByTemplateIndex : AbstractIndexCreationTask<CompleteQuestionnaireStatisticDocument, CQGroupItem>
    {
        public QuestionnaireGroupedByTemplateIndex()
        {
        
            Map = docs => from doc in docs
                          select new
                          {
                             Id = doc.TemplateId,
                             TotalCount = 1
                          };


            Reduce = results => from result in results
                                group result by result.Id
                                    into g
                                    select new
                                    {
                                        Id = g.Key,
                                        TotalCount = g.Sum(x => x.TotalCount)
                                    };
            TransformResults =
                 (database, questionnaires) => from questionnaire in questionnaires
                                               let alias = database.Load<QuestionnaireDocument>(questionnaire.Id)
                                      select new
                                      {
                                          Title = alias.Title,
                                          Id = questionnaire.Id,
                                          TotalCount = questionnaire.TotalCount
                                      };
        }
    }
    */
    public class QuestionnaireGroupedByTemplateIndex : AbstractMultiMapIndexCreationTask<CQGroupItem>
    {
        public QuestionnaireGroupedByTemplateIndex()
        {
            AddMap<QuestionnaireDocument>(templates => from template in templates
                                                       select new
                                                                  {
                                                                      Id = template.Id,
                                                                      SurveyTitle = template.Title,
                                                                      TotalCount = 0
                                                                  });

            AddMap<CompleteQuestionnaireStatisticDocument>(
                completeQuestionnaires => from completeQuestionnaire in completeQuestionnaires
                                          select new
                                                     {
                                                         Id = completeQuestionnaire.TemplateId,
                                                         SurveyTitle = completeQuestionnaire.Title,
                                                         TotalCount = 1
                                                     });
            


            Reduce = results => from result in results
                                 group result by result.Id
                                into g
                                select new
                                           {
                                               Id = g.Key,
                                               SurveyTitle = g.Select(x => x.SurveyTitle).First(x => x != null),
                                               TotalCount = g.Sum(x => x.TotalCount)
                                           };

            Index(x => x.SurveyTitle, FieldIndexing.Analyzed);
           /* Store(x => x.SurveyTitle, FieldStorage.Yes);
            Store(x => x.TotalCount, FieldStorage.Yes);**/
        }
    }
}
