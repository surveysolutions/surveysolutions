using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "QuestionnaireTitle_required")]
        [AllowHtml]
        public string Title { get; set; }

        public bool IsPublic { get; set; }

        [AllowHtml]
        public string SubTitle { get; set; }

        public StudyType? StudyType { get; set; }

        public string Version { get; set; }

        public string VersionNotes { get; set; }

        public string KindOfData { get; set; }

        public string Country { get; set; }

        public int? Year { get; set; }

        public string Language { get; set; }

        public string Coverage { get; set; }

        public string Universe { get; set; }

        public string UnitOfAnalysis { get; set; }

        public string PrimaryInvestigator { get; set; }

        public string Funding { get; set; }

        public string Consultant { get; set; }

        public ModeOfDataCollection? ModeOfDataCollection { get; set; }

        public string Notes { get; set; }

        public string Keywords { get; set; }

        public bool AgreeToMakeThisQuestionnairePubic { get; set; }
    }
}