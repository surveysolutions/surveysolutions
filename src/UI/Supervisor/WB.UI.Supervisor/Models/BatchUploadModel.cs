using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Models
{
    public class BatchUploadModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        [ValidateFile(ErrorMessage = "Please select file")]
        [Display(Name = "CSV File")]
        public HttpPostedFileBase File { get; set; }
        public FeaturedQuestionItem[] FeaturedQuestions { get; set; }
    }
}