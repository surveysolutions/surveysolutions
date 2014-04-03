using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
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
