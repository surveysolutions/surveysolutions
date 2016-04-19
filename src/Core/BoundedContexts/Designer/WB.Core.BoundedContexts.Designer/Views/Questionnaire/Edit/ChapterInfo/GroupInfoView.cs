using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class GroupInfoView : IQuestionnaireItem, IReadSideRepositoryEntity
    {
        public GroupInfoView()
        {
            this.Items = new List<IQuestionnaireItem>();
        }

        public string ItemId { get; set; }
        public ChapterItemType ItemType { get { return ChapterItemType.Group; }}

        public string Title { get; set; }

        public bool IsRoster { get; set; }
        public List<IQuestionnaireItem> Items { get; set; }
        public int QuestionsCount { get; set; }
        public int GroupsCount { get; set; }
        public int RostersCount { get; set; }
        public string Variable { get; set; }
        public bool HasCondition { get; set; }
    }
}
