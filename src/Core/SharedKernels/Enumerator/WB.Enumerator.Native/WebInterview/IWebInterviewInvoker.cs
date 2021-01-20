using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IWebInterviewInvoker
    {
        void RefreshEntities(string interviewId, string[] identities);
        void RefreshSection(Guid interviewId);
        void RefreshSectionState(Guid interviewId);
        void ReloadInterview(Guid interviewId);
        void FinishInterview(Guid interviewId);
        void MarkAnswerAsNotSaved(string section, string questionId, string errorMessage);
        void ShutDown(Guid interviewId);
        void ShutDownAllWebInterviews();
    }
}
