using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserEditModel
    {
        [Key]
        public Guid Id { get; set; }

        public string UserName { get; set; }

        [PasswordStringLength(100, ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordLengthMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [PasswordRegularExpression(ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.PasswordFieldName), ResourceType = typeof (FieldsAndValidations), Order = 1)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.ConfirmPasswordFieldName), ResourceType = typeof (FieldsAndValidations), Order = 2)]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
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

        public IList<DeviceInfo> DevicesHistory { get; set; }
    }
}