using System;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IUserPreconditionsService
    {
        bool IsUserNameTakenByActiveUsers(string userName);
        bool IsUserNameTakenByArchivedUsers(string userName);
        int CountOfInterviewsInterviewerResposibleFor(Guid interviewerId);
        bool IsUserActive(Guid userId);
    }
}