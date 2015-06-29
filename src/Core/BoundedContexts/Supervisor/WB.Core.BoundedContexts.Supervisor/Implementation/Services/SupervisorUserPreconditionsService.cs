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

        public int CountOfInterviewsUserResposibleFor(Guid userId)
        {
            return 0;
        }
    }
}