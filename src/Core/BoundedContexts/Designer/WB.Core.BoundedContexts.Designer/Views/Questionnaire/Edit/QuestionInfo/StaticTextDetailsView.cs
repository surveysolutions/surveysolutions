namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class StaticTextDetailsView : DescendantItemView
    {
        private string text;

        public string Text
        {
            get { return this.text; }
            set { this.text = System.Web.HttpUtility.HtmlDecode(value); }
        }
    }
}