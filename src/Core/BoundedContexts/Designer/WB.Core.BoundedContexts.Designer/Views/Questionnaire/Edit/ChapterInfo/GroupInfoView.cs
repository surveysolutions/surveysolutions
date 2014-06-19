using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class GroupInfoView : GroupInfoStatisticsView, IQuestionnaireItem, IReadSideRepositoryEntity
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public bool IsRoster { get; set; }
        public List<IQuestionnaireItem> Items { get; set; }
    }
}