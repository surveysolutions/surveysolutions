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
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class CQStatusReportFactory : IViewFactory<CQStatusReportViewInputModel, CQStatusReportView>
    {
      //  private IDocumentSession documentSession;

        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;
        public CQStatusReportFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
        }

        public CQStatusReportView Load(CQStatusReportViewInputModel input)
        {
          //  var statuses = documentSession.Query<StatusDocument>().FirstOrDefault(u => u.QuestionnaireId == input.QuestionnaireId);
            var statuseFirst = SurveyStatus.GetAllStatuses().FirstOrDefault();
            if (statuseFirst == null)
                return null; //no satelite status document 

            var status = new StatusItem(){ Title = statuseFirst.Name};// statuses.FirstOrDefault(s => s.PublicId == input.StatusId);}

            var query = this.documentItemSession.Query().Where(x => (x.TemplateId == input.QuestionnaireId) && (x.Status.PublicId == input.StatusId)).ToList();

            return new CQStatusReportView(status, query);
        }
    }
}
