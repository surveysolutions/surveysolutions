using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class CQStatusReportFactory : IViewFactory<CQStatusReportViewInputModel, CQStatusReportView>
    {
        //  private IDocumentSession documentSession;

        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        public CQStatusReportFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        #region IViewFactory<CQStatusReportViewInputModel,CQStatusReportView> Members

        public CQStatusReportView Load(CQStatusReportViewInputModel input)
        {
            //  var statuses = documentSession.Query<StatusDocument>().FirstOrDefault(u => u.QuestionnaireId == input.QuestionnaireId);
            SurveyStatus statuseFirst = SurveyStatus.GetAllStatuses().FirstOrDefault();
            if (statuseFirst == null)
                return null; //no satelite status document 

            var status = new StatusItem {Title = statuseFirst.Name};
                // statuses.FirstOrDefault(s => s.PublicId == input.StatusId);}

            List<CompleteQuestionnaireBrowseItem> query =
                documentItemSession.Query().Where(
                    x => (x.TemplateId == input.QuestionnaireId) && (x.Status.PublicId == input.StatusId)).ToList();

            return new CQStatusReportView(status, query);
        }

        #endregion
    }
}