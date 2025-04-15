#nullable enable
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public interface IExportSettings
    {
        bool EncryptionEnforced();
        string GetPassword();

        void SetEncryptionEnforcement(bool value);
        void RegeneratePassword();

        ExportEncryptionSettings? GetEncryptionSettings();
        
        ExportRetentionSettings? GetExportRetentionSettings();
        void SetExportRetentionSettings(bool enabled, int? daysToKeep, int? countToKeep);
    }
}
