using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireTitle_required")]
        [AllowHtml]
        public string Title { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireVariable_required")]
        [RegularExpression("^(?!.*[_]{2})[A-Za-z][_A-Za-z0-9]*(?<!_)$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_rules))]
        [StringLength(32, ErrorMessageResourceName = nameof(ErrorMessages.QuestionnaireVariable_MaxLength), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
        public string Variable { get; set; }

        public bool IsPublic { get; set; }
    }
}
