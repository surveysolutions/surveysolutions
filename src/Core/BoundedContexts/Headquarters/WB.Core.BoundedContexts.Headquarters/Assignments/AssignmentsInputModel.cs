using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentsInputModel : ListViewModelBase
    {
        public SearchTypes SearchByFields { get; set; } = SearchTypes.Id | SearchTypes.ResponsibleId | SearchTypes.IdentifyingQuestions;

        public string SearchBy { get; set; }
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
        public Guid? SupervisorId { get; set; }
        public bool ShowArchive { get; set; }
        public bool ShowQuestionnaireTitle { get; set; } = false;
        public bool OnlyWithInterviewsNeeded { get; set; } = false;
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public UserRoles? UserRole { get; set; }
        public AssignmentReceivedState ReceivedByTablet { get; set; }

        [Flags]
        public enum SearchTypes
        {
            Id, ResponsibleId, IdentifyingQuestions, QuestionnaireTitle
        }
    }
}
