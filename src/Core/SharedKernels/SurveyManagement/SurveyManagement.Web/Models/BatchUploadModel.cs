using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class BatchUploadModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public string QuestionnaireTitle { get; set; }

        [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = "BatchUploadModel_Required")]
        [ValidateFile(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = "BatchUploadModel_ValidationErrorMessage")]
        [Display(ResourceType = typeof(FieldsAndValidations), Name = "BatchUploadModel_FileName")]
        public HttpPostedFileBase File { get; set; }
        public List<FeaturedQuestionItem> FeaturedQuestions { get; set; }
    }
}