using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    [DisplayName("Create Questionnaire")]
    public class QuestionnaireViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
    }
}