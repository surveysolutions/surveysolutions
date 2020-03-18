namespace WB.UI.Headquarters.Models
{
    public class SurveySetupModel
    {
        public string SubTitle { get; set; }
        public string Title { get; set; }
        public string DataUrl { get; set; }
        public bool IsObserver { get; set; }
        public bool IsAdmin { get; set; }
        public string QuestionnaireDetailsUrl { get; set; }
        public string TakeNewInterviewUrl { get; set; }
        public string BatchUploadUrl { get; set; }
        public string MigrateAssignmentsUrl { get; set; }
        public string WebInterviewUrl { get; set; }
        public string DownloadLinksUrl { get; set; }
        public string SendInvitationsUrl { get; set; }
        public string CloneQuestionnaireUrl { get; set; }
        public string ExportQuestionnaireUrl { get; set; }
        public string ImportQuestionnaireUrl { get; set; }
    }
}
