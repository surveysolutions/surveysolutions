using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserPreloadingVerifier
    {
        PreloadedDataValidator[] GetAllUsersValidations(UserToValidate[] allInterviewersAndSupervisors,
            IList<UserPreloadingDataRecord> usersToImport);

        PreloadedDataValidator[] GetEachUserValidations(UserToValidate[] allInterviewersAndSupervisors);
    }
}