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
    public class CQStatusReportFactory : IViewFactory<CQStatusReportViewInputModel, CQStatusReportView>
    {
        private IDocumentSession documentSession;


        public CQStatusReportFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public CQStatusReportView Load(CQStatusReportViewInputModel input)
        {
            var statuses = documentSession.Query<StatusDocument>().FirstOrDefault(u => u.QuestionnaireId == input.QuestionnaireId);

            if (statuses == null)
                return null; //no satelite status document 

            var status = statuses.Statuses.FirstOrDefault(s => s.PublicKey == input.StatusId);

            var query = documentSession.Query<CompleteQuestionnaireStatisticDocument>().Where(x => (x.TemplateId == input.QuestionnaireId) && (x.Status.PublicId == input.StatusId)).ToList();

            return new CQStatusReportView(status, query);
        }
    }
}
