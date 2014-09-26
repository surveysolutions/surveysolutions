using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
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
