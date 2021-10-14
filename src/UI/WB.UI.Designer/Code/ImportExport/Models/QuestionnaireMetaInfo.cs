using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class QuestionnaireMetaInfo
    {
        public string? SubTitle { get; set; }

        public StudyType? StudyType { get; set; }

        public string? Version { get; set; }

        public string? VersionNotes { get; set; }

        public string? KindOfData { get; set; }

        public string? Country { get; set; }

        public int? Year { get; set; }

        public string? Language { get; set; }

        public string? Coverage { get; set; }

        public string? Universe { get; set; }

        public string? UnitOfAnalysis { get; set; }

        public string? PrimaryInvestigator { get; set; }

        public string? Funding { get; set; }

        public string? Consultant { get; set; }

        public ModeOfDataCollection? ModeOfDataCollection { get; set; }

        public string? Notes { get; set; }

        public string? Keywords { get; set; }

        public bool AgreeToMakeThisQuestionnairePublic { get; set; }
    }
}
