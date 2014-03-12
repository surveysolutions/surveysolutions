using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Models
{
    public class NewSurveyModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}