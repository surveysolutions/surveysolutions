using System;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Models.Users
{
    public class UserEditModel : UserModelBase, IPasswordRequired
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        //[EmailAddress(ErrorMessageResourceName = nameof(FieldsAndValidations.EmailErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        public string Email { get; set; }

        //[StringLength(UserModel.PersonNameMaxLength, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessage = null)]
        //[ServerSideOnlyRegularExpression(UserModel.PersonNameRegex, ErrorMessageResourceName = nameof(FieldsAndValidations.PersonNameAllowedCharactersErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string PersonName { get; set; }

        //[Phone(ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorMessage), ErrorMessageResourceType = typeof(FieldsA
        //[StringLength(UserModel.PhoneNumberLength, ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.PhoneErrorLength))]
        public string PhoneNumber { get; set; }

        public bool IsLocked { get; set; }

        public bool IsLockedBySupervisor { get; set; }

        public string UpdatePasswordAction { get; set; } = @"UpdatePassword";

        public bool AllowEditPassword { get; set; } = true;
        public bool AllowEditLockState { get; set; } = true;
    }
}
