using System;
using System.ComponentModel.DataAnnotations;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models.Users
{
    public class CreateUserModel : EditUserModel, IPasswordRequired
    {
        public const string UserNameRegularExpression = "^[a-zA-Z0-9_]{3,15}$";

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [RegularExpression(UserNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserNameErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        //[Remote("IsUniqueUsername", "Account", HttpMethod = "POST", ErrorMessageResourceName = nameof(FieldsAndValidations.UserName_Taken), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string UserName { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public Guid? SupervisorId { get; set; }
        public string Role { get; set; }
    }
}
