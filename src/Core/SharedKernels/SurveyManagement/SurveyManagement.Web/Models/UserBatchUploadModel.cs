using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class UserBatchUploadModel
    {
        [Required(ErrorMessage = "You must choose a file to upload")]
        [ValidateFile(ErrorMessage = "Please select file")]
        [Display(Name = "CSV File")]
        public HttpPostedFileBase File { get; set; }

        public string[] AvaliableDataColumnNames { get; set; }
    }
}