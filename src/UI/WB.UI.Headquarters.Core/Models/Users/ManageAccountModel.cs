using System;

namespace WB.UI.Headquarters.Models.Users
{
    public class ManageAccountModel : UserEditModel
    {
        //[DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        //[DataType(DataType.Text)]
        public string Role { get; set; }
    }

    public class ManageAccountNewModel
    {
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

        //[DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        //[DataType(DataType.Text)]
        public string Role { get; set; }
    }
}
