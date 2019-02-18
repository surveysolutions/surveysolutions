using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IQuestionStateViewModel : IDisposable
    {
        QuestionHeaderViewModel Header { get; }
        ValidityViewModel Validity { get; }
        WarningsViewModel Warnings { get; }
        EnablementViewModel Enablement { get; }
        CommentsViewModel Comments { get; }
        void Init(string interviewId, Identity entityIdentity, NavigationState navigationState);
    }
}
