using System;
using System.Net.Http;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public interface IExportSettings
    {
        bool EncryptionEnforced();
        string GetPassword();

        void SetEncryptionEnforcement(bool value);
        void RegeneratePassword();
        string ExportServiceBaseUrl { get; }
        string ApiKey { get; }
    }
}
