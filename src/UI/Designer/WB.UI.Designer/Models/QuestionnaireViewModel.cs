using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireViewModel
    {
        [Required]
        [AllowHtml]
        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}