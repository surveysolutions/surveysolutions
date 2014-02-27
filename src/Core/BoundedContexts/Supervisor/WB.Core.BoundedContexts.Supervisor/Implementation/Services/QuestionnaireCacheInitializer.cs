using Main.Core.Documents;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.DataCollection.Factories;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
{
    internal class QuestionnaireCacheInitializer : IQuestionnaireCacheInitializer
    {
        private readonly IQuestionnaireFactory questionnaireFactory;

        public QuestionnaireCacheInitializer(IQuestionnaireFactory questionnaireFactory)
        {
            this.questionnaireFactory = questionnaireFactory;
        }

        public void InitializeQuestionnaireDocumentWithCaches(QuestionnaireDocument document)
        {
            var questionnaire = this.questionnaireFactory.CreateTemporaryInstance(document);
            questionnaire.InitializeQuestionnaireDocument();
        }
    }
}
