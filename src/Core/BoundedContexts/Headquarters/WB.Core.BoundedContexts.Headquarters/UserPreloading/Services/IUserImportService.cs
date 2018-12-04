using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public interface IUserImportService
    {
        IEnumerable<UserImportVerificationError> VerifyAndSaveIfNoErrors(byte[] data, string fileName);
        string[] GetUserProperties();
        UserToImport GetUserToImport();
        void RemoveImportedUser(UserToImport importedUser);
        UsersImportStatus GetImportStatus();
        UsersImportCompleteStatus GetImportCompleteStatus();
        void RemoveAllUsersToImport();
    }
}
