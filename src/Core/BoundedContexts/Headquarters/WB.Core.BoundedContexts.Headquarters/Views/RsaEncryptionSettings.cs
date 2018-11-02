using WB.Core.BoundedContexts.Headquarters.DataExport.Security;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class RsaEncryptionSettings : AppSetting
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
