using System.Collections.Generic;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader
{
    public interface IViewModelLoader
    {
        ChapterViewModel Load(string interviewId, string chapterId);
    }
}