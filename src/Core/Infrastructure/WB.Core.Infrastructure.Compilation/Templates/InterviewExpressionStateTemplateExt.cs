using Main.Core.Documents;

namespace WB.Core.Infrastructure.Compilation.Templates
{
    public partial class InterviewExpressionStateTemplate
    {
        protected QuestionnaireDocument questionnaireDocument;

        public InterviewExpressionStateTemplate(QuestionnaireDocument questionnaire)
        {
            this.questionnaireDocument = questionnaire;
        }
    }
}
