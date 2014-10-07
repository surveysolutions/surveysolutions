using System.ComponentModel.DataAnnotations;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserModel
    {
        [Required(ErrorMessageResourceName = "RequiredUserNameErrorMessage",
            ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [Display(Name = "UserNameFieldName", ResourceType = typeof (FieldsAndValidations), Order = 1)]
        [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessageResourceName = "UserNameErrorMessage",
            ErrorMessageResourceType = typeof (FieldsAndValidations))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = "RequiredPasswordErrorMessage",
            ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [PasswordStringLength(100, ErrorMessageResourceName = "PasswordLengthMessage", ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [PasswordRegularExpression(ErrorMessageResourceName = "PasswordErrorMessage", ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordFieldName", ResourceType = typeof (FieldsAndValidations), Order = 2)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPasswordFieldName", ResourceType = typeof (FieldsAndValidations), Order = 3)]
        [Compare("Password", ErrorMessageResourceName = "ConfirmPasswordErrorMassage",
            ErrorMessageResourceType = typeof (FieldsAndValidations))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceName = "RequiredEmailErrorMessage",
            ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [EmailAddress(ErrorMessageResourceName = "EmailErrorMessage",
            ErrorMessageResourceType = typeof (FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = "EmailFieldName", ResourceType = typeof (FieldsAndValidations), Order = 4)]
        public string Email { get; set; }

        [Display(Name = "IsLockedFieldName", ResourceType = typeof (FieldsAndValidations), Order = 5)]
        public bool IsLocked { get; set; }
    }
}