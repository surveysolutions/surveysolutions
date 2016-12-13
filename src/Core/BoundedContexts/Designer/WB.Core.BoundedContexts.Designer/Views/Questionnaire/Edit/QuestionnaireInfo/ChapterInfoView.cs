using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class ChapterInfoView : IQuestionnaireItem
    {
        public string ItemId { get; set; }
        public ChapterItemType ItemType => ChapterItemType.Chapter;
        public string Title { get; set; }
        public int QuestionsCount { get; set; }
        public int GroupsCount { get; set; }
        public int RostersCount { get; set; }

        public List<IQuestionnaireItem> Items
        {
            get
            {
                return new List<IQuestionnaireItem>(0);
            }
            set
            {
                // do nothing
            }
        }
    }
}