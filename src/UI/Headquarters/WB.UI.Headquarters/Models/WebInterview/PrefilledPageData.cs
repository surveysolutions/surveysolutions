namespace WB.UI.Headquarters.Models.WebInterview
{
    public class PrefilledPageData
    {
        public string FirstSectionId { get; set; }
        public InterviewEntityWithType[] Questions { get; set; }
    }

    public class SectionData
    {
        public SimpleGroupStatus Status { get; set; }

        public string Title { get; set; }

        public InterviewEntityWithType[] Entities { get; set; }
    }

    public enum SimpleGroupStatus
    {
        Completed = 1,
        Invalid = -1,
        Other = 0,
    }
}