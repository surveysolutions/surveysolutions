using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Indexes;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class StatusReportViewFactory : IViewFactory<StatusReportViewInputModel, StatusReportView>
    {
        private IDocumentSession documentSession;


        public StatusReportViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusReportView Load(StatusReportViewInputModel input)
        {
            var statusGroups = new List<StatusReportGroupView>();
            var query = documentSession.Query<QuestionnaireDocument>().ToList();

            foreach (var q in query)
            {
                var statusGroup = new StatusReportGroupView(q.Id, q.Title);

                var statuses = (from s in documentSession.Query<StatusDocument>()
                                         .FirstOrDefault(u => u.QuestionnaireId == q.Id).Statuses
                               select new StatusReportItemView
                                          {
                                              StatusId = s.PublicKey,
                                              StatusTitle = s.Title,
                                              QuestionnaireCount = 0
                                          }).ToList();

                var cqs_raw = documentSession.Query<CompleteQuestionnaireStatisticDocument>().Where(x => x.TemplateId == q.Id)
                        .Select(s => new { StatusId = s.Status.PublicId, Count = 1 }).ToList();

                var grouped = from cq in cqs_raw
                              group cq by cq.StatusId
                                  into g
                                  select new
                                      {
                                          StatusId = g.Key,
                                          QuestionnaireCount = g.Count()
                                      };

                foreach (var group in grouped)
                {
                    var status = statuses.FirstOrDefault(s => s.StatusId == group.StatusId);
                    if (status!=null)
                    {
                        status.QuestionnaireCount = group.QuestionnaireCount;
                    }
                }

                statusGroup.Items.AddRange(statuses);

                statusGroups.Add(statusGroup);
            }

            return new StatusReportView() { Items = statusGroups };
        }
    }
}
