using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects
{
    public class EmailProviderSettings : AppSetting, IAmazonEmailSettings, ISendGridEmailSettings, ISenderInformation
    {
        public EmailProvider Provider{ get; set; }
        public string SenderAddress{ get; set; }
        public string AwsAccessKeyId{ get; set; }
        public string AwsSecretAccessKey{ get; set; }
        public string AwsRegion{ get; set; } = "us-east-1";
        public string SendGridApiKey{ get; set; }
        public string SenderName{ get; set; }
        public string ReplyAddress{ get; set; }
        public string Address{ get; set; }
    }

    public interface ISendGridEmailSettings
    {
        string SenderAddress{ get; }
        string SendGridApiKey{ get; }
        string ReplyAddress { get; set; }
    }

    public interface IAmazonEmailSettings
    {
        string SenderAddress{ get; }
        string AwsAccessKeyId{ get; }
        string AwsSecretAccessKey{ get; }
        string AwsRegion{ get; }
        string ReplyAddress { get; set; }
    }

    public interface ISenderInformation
    {
        string SenderAddress{ get; }
        string SenderName{ get; }
        string ReplyAddress{ get; }
        string Address{ get; }
    }

    public enum EmailProvider
    {
        None = 0,
        Amazon = 1,
        SendGrid = 2
    }
}
