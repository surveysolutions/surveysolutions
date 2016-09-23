namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class FullGroupData : QuestionnaireActive
    {
        public string ConditionExpression { get; set; }
        public string GroupText { get; set; }
        public string VariableName { get; set; }
        public string Description { get; set; }
        public bool HideIfDisabled { get; set; }
    }
}