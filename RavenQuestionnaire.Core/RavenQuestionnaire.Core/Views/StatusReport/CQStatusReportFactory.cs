// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQStatusReportFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq status report factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

    /// <summary>
    /// The cq status report factory.
    /// </summary>
    public class CQStatusReportFactory : IViewFactory<CQStatusReportViewInputModel, CQStatusReportView>
    {
        // private IDocumentSession documentSession;
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQStatusReportFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public CQStatusReportFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
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
        /// The RavenQuestionnaire.Core.Views.StatusReport.CQStatusReportView.
        /// </returns>
        public CQStatusReportView Load(CQStatusReportViewInputModel input)
        {
            // var statuses = documentSession.Query<StatusDocument>().FirstOrDefault(u => u.QuestionnaireId == input.QuestionnaireId);
            SurveyStatus statuseFirst = SurveyStatus.GetAllStatuses().FirstOrDefault();
            if (statuseFirst == null)
            {
                return null; // no satelite status document 
            }

            var status = new StatusItem { Title = statuseFirst.Name };

            // statuses.FirstOrDefault(s => s.PublicId == input.StatusId);}
            List<CompleteQuestionnaireBrowseItem> query =
                this.documentItemSession.Query().Where(
                    x => (x.TemplateId == input.QuestionnaireId) && (x.Status.PublicId == input.StatusId)).ToList();

            return new CQStatusReportView(status, query);
        }

        #endregion
    }
}