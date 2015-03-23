using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.UI.Designer.Models
{
    using System.ComponentModel.DataAnnotations;

    public class PasswordModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Order = 3)]
        [Compare("Password", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "The_password_and_confirmation_password_do_not_match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [PasswordStringLength(100, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Password_must_be_at_least_characters_long")]
        [PasswordRegularExpression(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "Password_must_contain_at_least_one_number_one_upper_case_character_and_one_lower_case_character")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }
    }
}