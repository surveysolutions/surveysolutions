using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireViewModel
    {
        [Required]
        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}