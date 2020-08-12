using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models.ControlPanel
{
    public class MakeAdminViewModel
    {
        [Display(Name = "Login")]
        [Required]
        public string? Login { get; set; }
        [Display(Name = "Is admin")]
        public bool IsAdmin { get; set; }
    }
}
