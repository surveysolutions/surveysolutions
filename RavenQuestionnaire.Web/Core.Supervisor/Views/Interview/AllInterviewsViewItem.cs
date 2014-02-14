using System;

namespace Core.Supervisor.Views.Interview
{
    public class AllInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanDelete { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { set; get; }
    }
}