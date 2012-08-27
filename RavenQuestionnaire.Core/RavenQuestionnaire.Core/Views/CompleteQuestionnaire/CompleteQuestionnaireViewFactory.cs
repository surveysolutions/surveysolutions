using System;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewFactory : IViewFactory<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireView>
    {
        private IDocumentSession documentSession;

        public CompleteQuestionnaireViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
        public CompleteQuestionnaireView Load(CompleteQuestionnaireViewInputModel input)
        {
            if (input.CompleteQuestionnaireId != null && input.CompleteQuestionnaireId!= Guid.Empty)
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);

                return new CompleteQuestionnaireView(doc);
            }
         
            return null;
        }

    }
}
