using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Enumerator.Native.WebInterview.Models
{
    public class IdentifyingQuestion
    {
        public string Identity { get; set; }
        public string Answer { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public bool IsReadonly { get; set; }
    }

    public class PrefilledPageData
    {
        public InterviewEntityWithType[] Entities { get; set; }

        public string FirstSectionId { get; set; }

        public bool HasAnyQuestions { get; set; }
    }

    public class ButtonState : InterviewEntity
    {
        public GroupStatus Status { get; set; }
        public string Target { get; set; }
        public ButtonType Type { get; set; }
        public string RosterTitle { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public enum ButtonType
    {
        Start = 0, Next, Parent, Complete
    }

    public class BreadcrumbInfo
    {
        public Breadcrumb[] Breadcrumbs { get; set; }
        public GroupStatus Status { get; set; }
        public string Title { get; set; }
        public string RosterTitle { get; set; }
        public bool IsRoster { get; set; }
        public Validity Validity { get; set; } = new Validity();
    }

    public class Breadcrumb
    {
        public string Title { set; get; }
        public string RosterTitle { get; set; }
        public string Target { get; set; }
        public string ScrollTo { get; set; }
        public bool IsRoster { get; set; }
    }
}
