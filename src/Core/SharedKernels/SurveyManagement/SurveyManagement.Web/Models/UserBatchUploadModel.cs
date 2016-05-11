using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserBatchUploadModel
    {
        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = "UserBatchUploadModel_Required_ErrorMessage")]
        [ValidateFile(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = "UserBatchUploadModel_Validation_ErrorMessage")]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = "UserBatchUploadModel_File_CSV_File")]
        public HttpPostedFileBase File { get; set; }

        public string[] AvaliableDataColumnNames { get; set; }
    }
}