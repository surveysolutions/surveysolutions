using System;
using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.DataAnnotations;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public interface IPasswordRequired
    {
        [PasswordStringLength(100, ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordLengthMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [PasswordRegularExpression(ErrorMessageResourceName = nameof(FieldsAndValidations.PasswordErrorMessage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        [DataType(DataType.Password)]
        
        string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessageResourceName = nameof(FieldsAndValidations.ConfirmPasswordErrorMassage), ErrorMessageResourceType = typeof(FieldsAndValidations))]
        string ConfirmPassword { get; set; }
        
        Guid Id { get; set; }
        string UserName { get; set; }
        string UpdatePasswordAction { get; set; }
    }
}