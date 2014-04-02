using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Factories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
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
