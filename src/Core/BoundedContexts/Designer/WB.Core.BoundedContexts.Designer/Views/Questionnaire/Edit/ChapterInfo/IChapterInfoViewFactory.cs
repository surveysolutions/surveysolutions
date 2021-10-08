using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public interface IChapterInfoViewFactory
    {
        NewChapterView? Load(QuestionnaireRevision questionnaireId, string chapterId);
    }
}
