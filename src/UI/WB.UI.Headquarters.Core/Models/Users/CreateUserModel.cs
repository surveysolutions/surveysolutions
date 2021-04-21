using System;
using System.ComponentModel.DataAnnotations;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Models.Users
{
    public class CreateUserModel : EditUserModel
    {
        public const string UserNameRegularExpression = "^[a-zA-Z0-9_]{3,15}$";

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredUserNameErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [RegularExpression(UserNameRegularExpression, ErrorMessageResourceName = nameof(FieldsAndValidations.UserNameErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.RequiredPasswordErrorMessage), ErrorMessageResourceType = typeof (FieldsAndValidations))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordRequired), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public Guid? SupervisorId { get; set; }
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.UserRoleRequired), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string Role { get; set; }
        [Required(ErrorMessageResourceName = nameof(FieldsAndValidations.WorkspaceRequired), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        public string Workspace { get; set; }
    }
}
