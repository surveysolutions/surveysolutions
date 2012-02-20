using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.FlowGraph
{
    public class FlowGraphViewFactory : IViewFactory<FlowGraphViewInputModel, FlowGraphView>
    {
        private readonly IDocumentSession documentSession;

        public FlowGraphViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        #region IViewFactory<FlowGraphViewInputModel,FlowGraphView> Members

        public FlowGraphView Load(FlowGraphViewInputModel input)
        {
            var flowGraph = documentSession.Include<FlowGraphDocument>(x => x.QuestionnaireDocumentId).Load(input.FlowGraphId);

            QuestionnaireDocument questionnaire;
            if (flowGraph == null)
            {
                documentSession.Store(new FlowGraphDocument() { Id = input.FlowGraphId });
                questionnaire = documentSession.Load<QuestionnaireDocument>(input.QuestionnaireId);
            }
            else
            {
                questionnaire = documentSession.Load<QuestionnaireDocument>(flowGraph.QuestionnaireDocumentId);
            }
            return new FlowGraphView(flowGraph, new QuestionnaireView(questionnaire));
        }
        #endregion
    }
}