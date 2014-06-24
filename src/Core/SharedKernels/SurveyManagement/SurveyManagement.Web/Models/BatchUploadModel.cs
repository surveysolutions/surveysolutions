using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class BatchUploadModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        [Required(ErrorMessage = "You must choose a file to upload")]
        [ValidateFile(ErrorMessage = "Please select file")]
        [Display(Name = "CSV File")]
        public HttpPostedFileBase File { get; set; }
        public FeaturedQuestionItem[] FeaturedQuestions { get; set; }
    }
}