using System.ComponentModel.DataAnnotations;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireTitle_required")]
        [RegularExpression(QuestionnaireVerifications.QuestionnaireTitleRegularExpression, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_rules))]
        [StringLength(AbstractVerifier.MaxTitleLength, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireTitle_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
        public string Title { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireVariable_required")]
        [RegularExpression(AbstractVerifier.VariableRegularExpression, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_rules))]
        [StringLength(AbstractVerifier.DefaultVariableLengthLimit, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
        public string Variable { get; set; }

        public bool IsPublic { get; set; }
    }
}
