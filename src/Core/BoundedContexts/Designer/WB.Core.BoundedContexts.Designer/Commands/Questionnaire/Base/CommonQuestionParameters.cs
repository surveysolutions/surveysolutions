namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public class CommonQuestionParameters
    {
        public string Title { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string Instructions { get; set; }
    }
}