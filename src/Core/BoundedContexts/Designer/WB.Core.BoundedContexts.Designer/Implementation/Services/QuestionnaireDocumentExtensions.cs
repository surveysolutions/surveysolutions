using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public static class QuestionnaireDocumentExtensions
    {
        public static ReadOnlyQuestionnaireDocument AsReadOnly(this QuestionnaireDocument questionnaire)
        {
            return new ReadOnlyQuestionnaireDocument(questionnaire);
        }
    }
}