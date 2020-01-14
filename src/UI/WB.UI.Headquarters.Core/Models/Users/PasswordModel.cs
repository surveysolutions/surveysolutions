using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.UI.Headquarters.Models.Users
{
    public class PasswordModel
    {
        [DataType(DataType.Password)]
        [PasswordStringLength(100, ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordLengthMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [PasswordRegularExpression(ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredPasswordErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string ConfirmPassword { get; set; }
    }
}
