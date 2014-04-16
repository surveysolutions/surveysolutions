using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoView : GroupInfoStatisticsView, IReadSideRepositoryEntity
    {
        public string QuestionnaireId { get; set; }
        public string Title { get; set; }
        public List<ChapterInfoView> Chapters { get; set; }
    }
}
