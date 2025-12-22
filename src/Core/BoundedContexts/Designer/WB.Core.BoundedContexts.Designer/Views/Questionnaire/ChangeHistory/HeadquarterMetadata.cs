namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class HeadquarterMetadata
    {
        public string? HostName { get; set; }
        public string? UserName { get; set; }
        public string? Version { get; set; }
        public string? Build { get; set; }
        public string? ImporterLogin { get; set; }
        public long   QuestionnaireVersion { get; set; }
        public float  TimeZoneMinutesOffset { get; set; }
        
        public string? InstanceId { get; set; }
    }
}
