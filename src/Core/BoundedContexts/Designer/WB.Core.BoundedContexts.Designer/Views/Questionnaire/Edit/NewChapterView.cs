using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewChapterView
    {
        public NewChapterView(IQuestionnaireItem? chapter = null, 
            VariableName[]? variableNames = null,
            bool isCover = false,
            bool isReadOnly = false,
            bool hasCondition = false,
            bool hideIfDisabled = false
            )
        {
            Chapter = chapter;
            VariableNames = variableNames?? new VariableName[0];
            IsCover = isCover;
            IsReadOnly = isReadOnly;
            HasCondition = hasCondition;
            HideIfDisabled = hideIfDisabled;
        }

        public IQuestionnaireItem? Chapter { set; get; }
        public VariableName[] VariableNames { set; get; }
        public bool IsCover { get; set; }
        public bool IsReadOnly { get; set; }

        public bool HasCondition { get; set; }
        public bool HideIfDisabled { get; set; }
    }
}
