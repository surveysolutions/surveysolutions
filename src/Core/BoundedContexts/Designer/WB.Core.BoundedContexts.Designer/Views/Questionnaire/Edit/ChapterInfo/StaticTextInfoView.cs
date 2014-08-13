namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class StaticTextInfoView : IQuestionnaireItem
    {
        private string text;
        public string ItemId { get; set; }
        public ChapterItemType ItemType { get { return ChapterItemType.StaticText; }}

        public string Text
        {
            get { return this.text; }
            set { this.text = System.Web.HttpUtility.HtmlDecode(value); }
        }
    }
}