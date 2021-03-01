using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public interface IUserImportVerifier
    {
        PreloadedDataValidator[] GetAllUsersValidations(UserToValidate[] allInterviewersAndSupervisors,
            IList<UserToImport> usersToImport);

        PreloadedDataValidator[] GetEachUserValidations(UserToValidate[] allInterviewersAndSupervisors,
            List<string> workspaces);
    }
}