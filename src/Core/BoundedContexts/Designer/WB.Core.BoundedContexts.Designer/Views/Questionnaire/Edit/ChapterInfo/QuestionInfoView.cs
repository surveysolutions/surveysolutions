using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class QuestionInfoView : IQuestionnaireItem
    {
        private string title;
        public string ItemId { get; set; }
        public ChapterItemType ItemType { get {return ChapterItemType.Question;}}

        public string Title
        {
            get { return this.title; }
            set { this.title = System.Web.HttpUtility.HtmlDecode(value); }
        }

        public string Variable { get; set; }
        public string LinkedToQuestionId { get; set; }
        public QuestionType Type { get; set; }
    }
}