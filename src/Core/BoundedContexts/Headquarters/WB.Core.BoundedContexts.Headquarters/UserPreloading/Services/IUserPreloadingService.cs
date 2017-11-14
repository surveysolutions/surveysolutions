using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserPreloadingService
    {
        IEnumerable<UserPreloadingVerificationError> VerifyAndSaveIfNoErrors(byte[] data, string fileName);
        string[] GetUserProperties();
        void Import();
        UsersImportStatus GetImportStatus();
    }
}