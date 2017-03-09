using System.ComponentModel.DataAnnotations;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ManageAccountModel : UserEditModel
    {
        [DataType(DataType.Password)]
        [Display(Name = nameof(FieldsAndValidations.OldPasswordFieldName), ResourceType = typeof(FieldsAndValidations))]
        public string OldPassword { get; set; }
    }
}