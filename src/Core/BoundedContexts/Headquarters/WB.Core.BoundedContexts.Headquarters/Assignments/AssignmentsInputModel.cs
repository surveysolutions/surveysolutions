using System;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentsInputModel : ListViewModelBase
    {
        public string SearchBy { get; set; }
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
        public Guid? SupervisorId { get; set; }
        public bool ShowArchive { get; set; }
        public bool OnlyWithInterviewsNeeded { get; set; } = false;
        public int? Limit { get; set; }
        public int? Offset { get; set; }
    }
}