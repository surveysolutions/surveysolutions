using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using Web.Supervisor.Controllers;

namespace Web.Supervisor.Models
{
    public class BatchUploadModel
    {
        public Guid QuestionnaireId { get; set; }

        public string QuestionnaireTitle { get; set; }

        public FeaturedQuestionItem[] FeaturedQuestions { get; set; }

        [ValidateFile(ErrorMessage = "Please select file")]
        [Display(Name = "CSV File")]
        public HttpPostedFileBase File { get; set; }
    }
}