using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class AppSetting : IStorableEntity
    {
        public static readonly string EncryptionSettingId = "exportencryptionsettings";
        public static readonly string CompanyLogoStorageKey = "company logo";
        public static readonly string VersionCheckingInfoKey = "versoinKey";
        public static readonly string GlobalNoticeKey = "settings";
        public static readonly string QuestionnaireVersionKey = "QuestionnaireVersion";
        public static readonly string InterviewerSettings = "InterviewerSettings";
        public static readonly string ExportServiceStorageKey = "ExportService.ApiKey";
        public static readonly string RsaKeysForEncryption = "Encryption.RsaKeys";
        public static readonly string EmailProviderSettings = "EmailProviderSettings";
        public static readonly string InvitationsDistributionStatus = "InvitationsDistributionStatus";
        public static readonly string DeviceNotificationsSettings = "DeviceNotificationsSettings";
        public static readonly string ProfileSettings = "ProfileSettings";
        public static readonly string WebInterviewSettings = "WebInterviewSettings";
        public static readonly string NatualKeySettings = "NatualKeySettings";

    }
}
