// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusReportViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The status report view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

    /// <summary>
    /// The status report view factory.
    /// </summary>
    public class StatusReportViewFactory : IViewFactory<StatusReportViewInputModel, StatusReportView>
    {
        // private IDocumentSession documentSession;
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IDenormalizerStorage<CQGroupItem> documentGroupSession;

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusReportViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public StatusReportViewFactory(
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession, 
            IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemSession = documentItemSession;
            this.documentGroupSession = documentGroupSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.StatusReport.StatusReportView.
        /// </returns>
        public StatusReportView Load(StatusReportViewInputModel input)
        {
            var statusGroups = new List<StatusReportGroupView>();
            List<CQGroupItem> query = this.documentGroupSession.Query().ToList();
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaires = this.documentItemSession.Query();

            foreach (CQGroupItem q in query)
            {
                var statusGroup = new StatusReportGroupView(q.SurveyId, q.SurveyTitle);

                List<StatusReportItemView> statuses =
                    SurveyStatus.GetAllStatuses().Select(
                        s =>
                        new StatusReportItemView { StatusId = s.PublicId, StatusTitle = s.Name, QuestionnaireCount = 0 })
                        .ToList();

                var cqs_raw =
                    questionnaires.Where(x => x.TemplateId == q.SurveyId).Select(
                        s => new { StatusId = s.Status.PublicId, Count = 1 }).ToList();
                var grouped = from cq in cqs_raw
                              group cq by cq.StatusId
                              into g select new { StatusId = g.Key, QuestionnaireCount = g.Count() };

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

            return new StatusReportView { Items = statusGroups };
        }

        #endregion
    }
}