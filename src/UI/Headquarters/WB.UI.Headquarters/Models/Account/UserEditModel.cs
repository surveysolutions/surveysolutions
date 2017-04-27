using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserEditModel : UserModelBase, IPasswordRequired
    {
        [Key]
        public Guid Id { get; set; }

        public string UserName { get; set; }
        
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessageResourceName = nameof(FieldsAndValidations.EmailErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.EmailFieldName), ResourceType = typeof(FieldsAndValidations), Order = 3)]
        public string Email { get; set; }

        [StringLength(UserModel.PersonNameMaxLength, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.PersonNameFieldName), ResourceType = typeof(FieldsAndValidations), Order = 4)]
        [RegularExpression(UserModel.PersonNameRegex, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameAllowedCharactersErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string PersonName { get; set; }

        [Phone(ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = nameof(FieldsAndValidations.PhoneNumberFieldName), ResourceType = typeof(FieldsAndValidations), Order = 5)]
        [StringLength(UserModel.PhoneNumberLength, ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorLength))]
        public string PhoneNumber { get; set; }

        [Display(Name = nameof(FieldsAndValidations.IsLockedFieldName), ResourceType = typeof (FieldsAndValidations), Order = 6)]
        public bool IsLocked { get; set; }

        [Display(Name = nameof(FieldsAndValidations.IsLockedBySupervisorFieldName), ResourceType = typeof(FieldsAndValidations), Order = 7)]
        public bool IsLockedBySupervisor { get; set; }

        public string UpdatePasswordAction { get; set; } = @"UpdatePassword";
    }
}