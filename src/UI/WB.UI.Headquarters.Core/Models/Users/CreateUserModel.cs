using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models.Users
{
    public class CreateUserModel : NewUserModel
    {
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredPasswordErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
