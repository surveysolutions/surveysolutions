using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public interface IChapterInfoViewFactory
    {
        NewChapterView Load(string questionnaireId, string groupId);
    }
}