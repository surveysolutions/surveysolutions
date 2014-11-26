using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    public class MakeAdminViewModel
    {
        [Display(Name = "Login")]
        public string Login { get; set; }
        [Display(Name = "Is admin")]
        public bool IsAdmin { get; set; }
    }
}