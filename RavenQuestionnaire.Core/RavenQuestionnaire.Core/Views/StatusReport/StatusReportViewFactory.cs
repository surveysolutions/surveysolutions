using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Indexes;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.ViewSnapshot;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class StatusReportViewFactory : IViewFactory<StatusReportViewInputModel, StatusReportView>
    {
      //  private IDocumentSession documentSession;

        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;
        public StatusReportViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }

        public StatusReportView Load(StatusReportViewInputModel input)
        {
            var statusGroups = new List<StatusReportGroupView>();
            var query = this.documentGroupSession.Query().ToList();
            var questionnaires = this.documentItemSession.Query();

            foreach (var q in query)
            {
                var statusGroup = new StatusReportGroupView(q.SurveyId, q.SurveyTitle);

              /*  var statuses = (from s in documentSession.Query<StatusDocument>()
                                         .FirstOrDefault(u => u.QuestionnaireId == q.SurveyId).Statuses
                               select new StatusReportItemView
                                          {
                                              StatusId = s.PublicKey,
                                              StatusTitle = s.Title,
                                              QuestionnaireCount = 0
                                          }).ToList();*/
                var statuses = SurveyStatus.GetAllStatuses().Select(s => new StatusReportItemView()
                                                                             {
                                                                                 StatusId = s.PublicId,
                                                                                 StatusTitle = s.Name,
                                                                                 QuestionnaireCount = 0
                                                                             }).ToList();
                /*var cqs_raw = documentSession.Query<CompleteQuestionnaireStatisticDocument>().Where(x => x.TemplateId == q.Id)
                        .Select(s => new { StatusId = s.Status.PublicId, Count = 1 }).ToList();*/
                var cqs_raw = questionnaires.Where(x => x.TemplateId == q.SurveyId).Select(s => new { StatusId = s.Status.PublicId, Count = 1 }).ToList();
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
