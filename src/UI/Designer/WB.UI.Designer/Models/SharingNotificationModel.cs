using WB.Core.BoundedContexts.Designer.Views;

namespace WB.UI.Designer.Models
{
    public class SharingNotificationModel : IEmailNotification
    {
        public ShareChangeType ShareChangeType { get; set; }

        public string Email { get; set; }

        public string QuestionnaireId { get; set; }
        public string QuestionnaireDisplayTitle { get; set; }
        public string ShareTypeName { get; set; }
        public string UserCallName { get; set; }
        public string ActionPersonCallName { get; set; }
        public string SharedWithPersonEmail { get; set; }
    }
}
