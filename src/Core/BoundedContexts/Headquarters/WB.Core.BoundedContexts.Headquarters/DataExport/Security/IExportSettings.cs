using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public interface IExportSettings
    {
        bool EncryptionEnforced();
        string GetPassword();

        void SetEncryptionEnforcement(bool value);
        void RegeneratePassword();
        Task RemoveExportCache();
    }
}
