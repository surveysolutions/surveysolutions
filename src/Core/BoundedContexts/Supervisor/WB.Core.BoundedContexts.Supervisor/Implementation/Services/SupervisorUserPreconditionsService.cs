using System;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
{
    internal class SupervisorUserPreconditionsService : IUserPreconditionsService
    {
        public bool IsUserNameTakenByActiveUsers(string userName)
        {
            return false;
        }

        public bool IsUserNameTakenByArchivedUsers(string userName)
        {
            return false;
        }

        public int CountOfInterviewsInterviewerResposibleFor(Guid interviewerId)
        {
            return 0;
        }

        public bool IsUserActive(Guid userId)
        {
            return true;
        }
    }
}