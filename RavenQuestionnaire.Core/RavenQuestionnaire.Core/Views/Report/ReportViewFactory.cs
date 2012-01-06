using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportViewFactory: IViewFactory<ReportViewInputModel, ReportView>
    {
        private IDocumentSession documentSession;


        public ReportViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public ReportView Load(ReportViewInputModel input)
        {
            ReportDocument doc = documentSession.Load<ReportDocument>(input.Id);
            
            return doc == null 
                ? null
                : new ReportView(doc.Id, doc.Title, doc.Description);
        }
    }
}
