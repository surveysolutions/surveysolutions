using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserEditModel
    {
        [Key]
        public Guid Id { get; set; }

        public string UserName { get; set; }

        [PasswordStringLength(100, ErrorMessageResourceName = "PasswordLengthMessage", ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [PasswordRegularExpression(ErrorMessageResourceName = "PasswordErrorMessage", ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [DataType(DataType.Password)]
        [Display(Name = "PasswordFieldName", ResourceType = typeof (FieldsAndValidations), Order = 1)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPasswordFieldName", ResourceType = typeof (FieldsAndValidations), Order = 2)]
        [Compare("Password", ErrorMessageResourceName = "ConfirmPasswordErrorMassage",
            ErrorMessageResourceType = typeof (FieldsAndValidations))]
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessageResourceName = "EmailErrorMessage",
            ErrorMessageResourceType = typeof (FieldsAndValidations), ErrorMessage = null)]
        [Display(Name = "EmailFieldName", ResourceType = typeof (FieldsAndValidations), Order = 3)]
        public string Email { get; set; }

        [Display(Name = "IsLockedFieldName", ResourceType = typeof (FieldsAndValidations), Order = 4)]
        public bool IsLocked { get; set; }

        public IList<DeviceInfo> DevicesHistory { get; set; }
    }
}