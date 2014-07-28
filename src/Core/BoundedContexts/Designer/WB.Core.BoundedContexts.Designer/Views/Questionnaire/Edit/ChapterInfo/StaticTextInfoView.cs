namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class StaticTextInfoView : IQuestionnaireItem
    {
        public string ItemId { get; set; }
        public ChapterItemType ItemType { get { return ChapterItemType.StaticText; }}
        public string Text { get; set; }
    }
}