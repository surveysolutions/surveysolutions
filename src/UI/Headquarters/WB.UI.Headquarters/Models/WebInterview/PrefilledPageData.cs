namespace WB.UI.Headquarters.Models.WebInterview
{
    public class PrefilledPageData
    {
        public string FirstSectionId { get; set; }
        public InterviewEntityWithType[] Questions { get; set; }
    }

    public class SectionData
    {
        public string Title { get; set; }

        public InterviewEntityWithType[] Entities { get; set; }
    }
}