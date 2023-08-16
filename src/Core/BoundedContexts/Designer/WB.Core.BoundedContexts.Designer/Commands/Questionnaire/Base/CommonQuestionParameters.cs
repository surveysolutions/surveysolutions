using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public class CommonQuestionParameters
    {
        public string? Title { get; set; }
        public string? VariableName { get; set; }
        public string? VariableLabel { get; set; }
        public string? EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string? Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public string? OptionsFilterExpression { get; set; }
        public GeometryType? GeometryType { get; set; }
        
        public GeometryInputMode? GeometryInputMode { get; set; }
        
        public bool? GeometryOverlapDetection { get; set; }
    }
}
