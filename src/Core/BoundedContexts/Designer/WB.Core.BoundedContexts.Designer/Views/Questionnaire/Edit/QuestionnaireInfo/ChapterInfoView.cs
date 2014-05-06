using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class ChapterInfoView : GroupInfoStatisticsView, IQuestionnaireItem
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
    }
}