using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models.Users
{
    public class ChangePasswordByNameModel : PasswordModel
    {
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [RegularExpression(CreateUserModel.UserNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string UserName { get; set; }
    }
}
