using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models.Users
{
    public class NewUserModel : EditUserModel
    {
        public const string UserNameRegularExpression = "^[a-zA-Z0-9_]{3,15}$";

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [RegularExpression(UserNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string UserName { get; set; }

        public Guid? SupervisorId { get; set; }
        public string Role { get; set; }
    }
}
