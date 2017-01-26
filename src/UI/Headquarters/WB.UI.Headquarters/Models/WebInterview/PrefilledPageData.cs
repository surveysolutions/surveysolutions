namespace WB.UI.Headquarters.Models.WebInterview
{
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
    }

    public class Breadcrumb
    {
        public string Title { set; get; }
        public string Target { get; set; }
        public string ScrollTo { get; set; }
    }

    public enum SimpleGroupStatus
    {
        Completed = 1,
        Invalid = -1,
        Other = 0,
    }
}