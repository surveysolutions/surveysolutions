namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [DisplayName("Please log in")]
    public class LoginModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }

        [Display(Name = "Remember me?", Order = 3)]
        public bool RememberMe { get; set; }

        [Required]
        [Display(Name = "User name", Order = 1)]
        public string UserName { get; set; }
    }
}