using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserModel : UserModelBase
    {
        public const string UserNameRegularExpression = "^[a-zA-Z0-9_]{3,15}$";
        public const string PersonNameRegex = @"^[\p{L} '.-]+$";
        public const int PersonNameMaxLength = 100;
        public const int PhoneNumberLength = 15;

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [Display(Name = nameof(FieldsAndValidations.UserNameFieldName), ResourceType = typeof (FieldsAndValidations), Order = 1)]
        [RegularExpression(UserNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserNameErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [Remote("IsUniqueUsername", "Account", HttpMethod = "POST", ErrorMessageResourceName = nameof(FieldsAndValidations.UserName_Taken), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredPasswordErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [PasswordStringLength(100, ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordLengthMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [PasswordRegularExpression(ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.PasswordFieldName), ResourceType = typeof (FieldsAndValidations), Order = 2)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.ConfirmPasswordFieldName), ResourceType = typeof (FieldsAndValidations), Order = 3)]
        [System.ComponentModel.DataAnnotations.Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessageResourceName = nameof(FieldsAndValidations.EmailErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.EmailFieldName), ResourceType = typeof (FieldsAndValidations), Order = 4)]
        public string Email { get; set; }

        [StringLength(PersonNameMaxLength, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.PersonNameFieldName), ResourceType = typeof(FieldsAndValidations), Order = 5)]
        [ServerSideOnlyRegularExpressionAttribute(PersonNameRegex, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameAllowedCharactersErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string PersonName { get; set; }

        [Phone(ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.PhoneNumberFieldName), ResourceType = typeof(FieldsAndValidations), Order = 6)]
        [StringLength(PhoneNumberLength, ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorLength))]
        public string PhoneNumber { get; set; }

        [Display(Name = nameof(FieldsAndValidations.IsLockedFieldName), ResourceType = typeof (FieldsAndValidations), Order = 7)]
        public bool IsLocked { get; set; }
    }
}
