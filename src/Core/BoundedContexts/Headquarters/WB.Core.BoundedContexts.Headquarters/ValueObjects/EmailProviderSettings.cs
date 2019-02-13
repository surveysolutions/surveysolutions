using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects
{
    public class EmailProviderSettings : AppSetting
    {
        public EmailProvider Provider{ get; set; }
        public string SenderAddress{ get; set; }
        public string AwsAccessKeyId{ get; set; }
        public string AwsSecretAccessKey{ get; set; }
        public string SendGridApiKey{ get; set; }
    }
    
    public enum EmailProvider
    {
        None = 0,
        Amazon = 1,
        SendGrid = 2
    }
}
