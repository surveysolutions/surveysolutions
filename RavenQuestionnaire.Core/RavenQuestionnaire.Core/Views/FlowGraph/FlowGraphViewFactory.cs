using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Group
{
    public class FlowGraphViewFactory : IViewFactory<FlowGraphViewInputModel, FlowGraphView>
    {
        private IDocumentSession documentSession;

        public FlowGraphViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
        public FlowGraphView Load(FlowGraphViewInputModel input)
        {
            var doc = documentSession.Load<QuestionnaireDocument>(input.QuestionnaireId);
            if (doc.FlowGraph == null)
                return null;
            return new FlowGraphView(doc);
        }
    }
}
