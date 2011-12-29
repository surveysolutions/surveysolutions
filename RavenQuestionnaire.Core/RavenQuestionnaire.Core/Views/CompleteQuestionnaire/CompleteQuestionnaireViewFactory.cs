using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

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
            if (!string.IsNullOrEmpty(input.CompleteQuestionnaireId))
            {
                var doc = documentSession.Load<CompleteQuestionnaireDocument>(input.CompleteQuestionnaireId);

                return new CompleteQuestionnaireView(doc);
            }
            if (!string.IsNullOrEmpty(input.TemplateQuestionanireId))
            {
                var doc = documentSession.Load<QuestionnaireDocument>(input.TemplateQuestionanireId);
                return new CompleteQuestionnaireView(doc);
            }
            return null;
        }

    }
}
