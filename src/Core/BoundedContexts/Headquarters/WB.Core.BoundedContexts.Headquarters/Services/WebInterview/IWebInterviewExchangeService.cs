using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Services.WebInterview
{
    public interface IWebInterviewExchangeService
    {
        void AnswersDeclaredInvalid(Guid interviewId, Identity[] questions);
        void AnswersDeclaredValid(Guid interviewId, Identity[] questions);
    }
}
