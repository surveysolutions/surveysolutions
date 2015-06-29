using System;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.Shared.Android.Services
{
    internal class AndroidPreconditionsService : IInterviewPreconditionsService, IUserPreconditionsService
    {
        public int? GetMaxAllowedInterviewsCount()
        {
            return null;
        }

        public int? GetInterviewsCountAllowedToCreateUntilLimitReached()
        {
            return null;
        }

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

        public int CountOfActiveInterviewersForSupervisor(Guid spervisorId)
        {
            return 0;
        }
    }
}
