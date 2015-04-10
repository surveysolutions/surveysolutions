namespace WB.UI.Designer.Models
{
    using System.ComponentModel.DataAnnotations;

    public class LocalPasswordModel : PasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }
}