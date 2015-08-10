﻿using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IInterviewViewModelFactory
    {
        IEnumerable<IInterviewEntityViewModel> GetEntities(string interviewId, Identity groupIdentity, NavigationState navigationState);
        IEnumerable<IInterviewEntityViewModel> GetPrefilledQuestions(string interviewId);
        IEnumerable<dynamic> GetCompleteScreenEntities(string interviewId);
        T GetNew<T>() where T : class;
    }
}