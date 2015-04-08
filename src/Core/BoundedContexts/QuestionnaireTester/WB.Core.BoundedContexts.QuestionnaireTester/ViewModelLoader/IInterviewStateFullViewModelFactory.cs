using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader
{
    public interface IInterviewStateFullViewModelFactory
    {
        IEnumerable<object> Load(string interviewId, string chapterId);
    }
}