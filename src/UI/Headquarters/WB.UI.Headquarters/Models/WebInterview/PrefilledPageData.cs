using System.Collections.Generic;

namespace WB.UI.Headquarters.Models.WebInterview
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
        public SimpleGroupStatus Status { get; set; }
        public string Target { get; set; }
        public ButtonType Type { get; set; }
    }

    public enum ButtonType
    {
        Start = 0, Next, Parent, Complete
    }

    public class BreadcrumbInfo
    {
        public Breadcrumb[] Breadcrumbs { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string RosterTitle { get; set; }
        public bool IsRoster { get; set; }
    }

    public class Breadcrumb
    {
        public string Title { set; get; }
        public string RosterTitle { get; set; }
        public string Target { get; set; }
        public string ScrollTo { get; set; }
        public bool IsRoster { get; set; }
    }

    public enum SimpleGroupStatus
    {
        Completed = 1,
        Invalid = -1,
        Other = 0,
    }
}