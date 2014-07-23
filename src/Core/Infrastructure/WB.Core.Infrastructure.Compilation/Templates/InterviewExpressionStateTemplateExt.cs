using Main.Core.Documents;

namespace WB.Core.Infrastructure.Compilation.Templates
{
    public partial class InterviewExpressionStateTemplate
    {
        protected QuestionnaireExecutorTemplateModel QuestionnaireTemplateStructure { private set; get; }
    
        public InterviewExpressionStateTemplate(QuestionnaireExecutorTemplateModel questionnaire)
        {
            this.QuestionnaireTemplateStructure = questionnaire;
        }
    }
}
