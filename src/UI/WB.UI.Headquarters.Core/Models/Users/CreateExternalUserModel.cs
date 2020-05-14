using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models.Users
{
    public class CreateExternalUserModel : NewUserModel
    {
        public const string UserExternalNameRegularExpression = @"^[a-zA-Z0-9@_\.]{3,100}$";

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredExternalUserNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [RegularExpression(UserExternalNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserExternalNameErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string ExternalUserName { get; set; }
    }
}
