using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class StatusReportViewFactory : IViewFactory<StatusReportViewInputModel, StatusReportView>
    {
        //  private IDocumentSession documentSession;

        private readonly IDenormalizerStorage<CQGroupItem> documentGroupSession;
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public StatusReportViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession,
                                       IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }

        #region IViewFactory<StatusReportViewInputModel,StatusReportView> Members

        public StatusReportView Load(StatusReportViewInputModel input)
        {
            var statusGroups = new List<StatusReportGroupView>();
            List<CQGroupItem> query = documentGroupSession.Query().ToList();
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaires = documentItemSession.Query();

            foreach (CQGroupItem q in query)
            {
                var statusGroup = new StatusReportGroupView(q.SurveyId, q.SurveyTitle);

                List<StatusReportItemView> statuses = SurveyStatus.GetAllStatuses().Select(s => new StatusReportItemView
                                                                                                    {
                                                                                                        StatusId = s.PublicId,
                                                                                                        StatusTitle = s.Name,
                                                                                                        QuestionnaireCount = 0
                                                                                                    }).ToList();
                
                var cqs_raw =
                    questionnaires.Where(x => x.TemplateId == q.SurveyId).Select(
                        s => new {StatusId = s.Status.PublicId, Count = 1}).ToList();
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
                    StatusReportItemView status = statuses.FirstOrDefault(s => s.StatusId == group.StatusId);
                    if (status != null)
                    {
                        status.QuestionnaireCount = group.QuestionnaireCount;
                    }
                }

                statusGroup.Items.AddRange(statuses);

                statusGroups.Add(statusGroup);
            }

            return new StatusReportView {Items = statusGroups};
        }

        #endregion
    }
}