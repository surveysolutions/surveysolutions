using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class FinishIntallationModel
    {
        public const string UserNameRegularExpression = "^[a-zA-Z0-9_]{3,15}$";

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Display(Name = nameof(FieldsAndValidations.UserNameFieldName), ResourceType = typeof(FieldsAndValidations), Order = 1)]
        [RegularExpression(UserNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredPasswordErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.PasswordFieldName), ResourceType = typeof(FieldsAndValidations), Order = 2)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.ConfirmPasswordFieldName), ResourceType = typeof(FieldsAndValidations), Order = 3)]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessageResourceName = nameof(FieldsAndValidations.EmailErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.EmailFieldName), ResourceType = typeof(FieldsAndValidations), Order = 4)]
        public string Email { get; set; }
    }
}
