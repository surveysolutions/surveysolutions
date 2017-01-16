namespace WB.UI.Headquarters.Models.WebInterview
{
    public class InterviewDetails
    {
        public SectionInfo[] Sections { get; set; }
    }
    
    public class SectionInfo
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public SimpleGroupStatus Status { get; set; }
        public string Title { get; set; }
    }

    public class SectionData
    {
        public SectionInfo Info { get; set; }
        public Breadcrumb[] Breadcrumbs { set; get; }
        public InterviewEntityWithType[] Entities { get; set; }
    }

    public class Breadcrumb
    {
        public string Title { set; get; }
    }

    public enum SimpleGroupStatus
    {
        Completed = 1,
        Invalid = -1,
        Other = 0,
    }
}