using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;

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
        public int Limit { get; set; }
        public int Offset { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public UserRoles? UserRole { get; set; }
        public AssignmentReceivedState ReceivedByTablet { get; set; }
        public int? Id { get; set; }
        public bool NonCawiOnly { set; get; }

        public AssignmentStatus[] Statuses { get; set; }

        /// <summary>
        /// Per-question filter conditions, each in "variable|field|operator,value" format
        /// where field|operator matches interview filter conventions:
        /// valueLowerCase|startsWith, valueLowerCase|eq, answerCode|eq, answerCode|neq, value|eq
        /// </summary>
        public AssignmentFilterCondition[] Conditions { get; set; }

        [Flags]
        public enum SearchTypes
        {
            Id = 1, ResponsibleId = 2, IdentifyingQuestions = 4, QuestionnaireTitle = 8
        }
    }

    public class AssignmentFilterCondition
    {
        public string Variable { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
