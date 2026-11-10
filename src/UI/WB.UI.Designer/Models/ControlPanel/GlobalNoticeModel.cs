using System.ComponentModel.DataAnnotations;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models.ControlPanel
{
    public class GlobalNoticeModel
    {
        [Display(Name = nameof(QuestionnaireController.GlobalNoticeMessage), ResourceType = typeof(QuestionnaireController))]
        [StringLength(GlobalNoticeService.MaxMessageLength,
            ErrorMessageResourceName = nameof(QuestionnaireController.GlobalNoticeMessageTooLong),
            ErrorMessageResourceType = typeof(QuestionnaireController))]
        public string? Message { get; set; }
    }
}
