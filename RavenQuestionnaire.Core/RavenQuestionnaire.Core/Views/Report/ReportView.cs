namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public  ReportView ()
        {
        }

        public static ReportView New()
        {
            return new ReportView();
        }

        public  ReportView (string id, string title, string description):this()
        {
            this.Id = id;
            this.Title = Title;
            this.Description = description;
        }

    }
}
