using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class ChapterInfoView : IQuestionnaireItem
    {
        public ChapterInfoView(string itemId, string title, int questionsCount, int groupsCount, int rostersCount,
            bool isCover, bool isReadOnly)
        {
            ItemId = itemId;
            Title = title;
            QuestionsCount = questionsCount;
            GroupsCount = groupsCount;
            RostersCount = rostersCount;
            IsCover = isCover;
            IsReadOnly = isReadOnly;
        }

        public string ItemId { get; set; }
        public ChapterItemType ItemType => ChapterItemType.Chapter;
        public string Title { get; set; }
        public int QuestionsCount { get; set; }
        public int GroupsCount { get; set; }
        public int RostersCount { get; set; }
        public bool IsCover { get; set; }
        public bool IsReadOnly { get; set; }

        public List<IQuestionnaireItem> Items
        {
            get => new List<IQuestionnaireItem>(0);
            set
            {
                // do nothing
            }
        }

        public bool HasCondition
        {
            get => false;
            set { }
        }

        public bool HideIfDisabled
        {
            get => false;
            set { }
        }
    }
}
