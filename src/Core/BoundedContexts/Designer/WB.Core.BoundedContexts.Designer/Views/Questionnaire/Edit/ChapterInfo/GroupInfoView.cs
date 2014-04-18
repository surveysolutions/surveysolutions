using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class GroupInfoView : GroupInfoStatisticsView, IQuestionnaireItem, IReadSideRepositoryEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<IQuestionnaireItem> Items { get; set; }
    }
}