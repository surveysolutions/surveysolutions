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
        public string Variable { get; set; }

        public bool IsPublic { get; set; }
    }
}
