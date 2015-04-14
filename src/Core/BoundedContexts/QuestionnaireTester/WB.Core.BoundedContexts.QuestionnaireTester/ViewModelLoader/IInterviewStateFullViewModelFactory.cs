using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader
{
    public interface IInterviewStateFullViewModelFactory
    {
        IEnumerable<MvxViewModel> Load(Guid interviewId, string chapterId);
    }
}