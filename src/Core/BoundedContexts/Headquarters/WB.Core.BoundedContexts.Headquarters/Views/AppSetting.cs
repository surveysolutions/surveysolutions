using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class AppSetting : IStorableEntity
    {
        public static readonly string EncriptionSettingId = "exportencryptionsettings";
        public static readonly string CompanyLogoStorageKey = "company logo";
        public static readonly string VersionCheckingInfoKey = "versoinKey";
        public static readonly string GlobalNoticeKey = "settings";
        public static readonly string QuestionnaireVersionKey = "QuestionnaireVersion";
        public static readonly string InterviewerSettings = "InterviewerSettings";
        public static readonly string ExportServiceStorageKey = "ExportService.ApiKey";
        public static readonly string RsaKeysForEncryption = "Encryption.RsaKeys";
    }
}