using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public interface IUserImportService
    {
        IEnumerable<UserImportVerificationError> VerifyAndSaveIfNoErrors(Stream data, string fileName, string workspace);
        string[] GetUserProperties();
        UserToImport GetUserToImport();
        void RemoveImportedUser(UserToImport importedUser);
        UsersImportStatus GetImportStatus();
        UsersImportCompleteStatus GetImportCompleteStatus();
        void RemoveAllUsersToImport();
    }
}
