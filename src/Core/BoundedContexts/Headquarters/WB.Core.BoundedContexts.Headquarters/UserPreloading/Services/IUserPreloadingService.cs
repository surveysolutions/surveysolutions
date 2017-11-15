using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserPreloadingService
    {
        IEnumerable<UserPreloadingVerificationError> VerifyAndSaveIfNoErrors(byte[] data, string fileName);
        string[] GetUserProperties();
        Task<bool> ImportFirstUserAndReturnIfHasMoreUsersToImportAsync();
        UsersImportStatus GetImportStatus();
        UsersImportCompleteStatus GetImportCompleteStatus();
        void RemoveAllUsersToImport();
    }
}