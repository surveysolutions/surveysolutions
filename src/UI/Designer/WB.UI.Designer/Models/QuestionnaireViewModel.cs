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

        public bool IsPublic { get; set; }
    }
}
