using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models
{
    public class LoginModel
    {
        [Required]
        [Display(ResourceType = typeof(LoginPageResources), Name = "Login")]
        public string Login { get; set; }

        [Display(ResourceType = typeof(LoginPageResources), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(LoginPageResources), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }
}