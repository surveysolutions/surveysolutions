namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [DisplayName("Password reset")]
    public class ResetPasswordModel
    {
        [Required]
        [Display(Name = "User Name", Order = 1)]
        public string UserName { get; set; }
    }
}