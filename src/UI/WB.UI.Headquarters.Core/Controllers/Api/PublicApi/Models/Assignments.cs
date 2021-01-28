using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignmentDetails : AssignmentViewItem
    {
        [DataMember] public List<AssignmentIdentifyingDataItem> IdentifyingData { get; set; }
    }

    public class InterviewAnswer
    {
        public Identity Identity { get; set; }
        public AbstractAnswer Answer { get; set; }
        public string Variable { get; set; }
    }

    public class FullAssignmentDetails : AssignmentDetails
    {
        [DataMember] 
        public List<InterviewAnswer> Answers { get; set; }
    }

    public class AssignmentIdentifyingDataItem
    {
        /// <summary>
        /// Question identity
        /// Expected format: GuidWithoutDashes_Int1-Int2, where _Int1-Int2 - codes of parent rosters (empty if question is not inside any roster).
        /// For example: 11111111111111111111111111111111_0-1 should be used for question on the second level
        /// </summary>
        [DataMember]
        public string Identity { get; set; }

        [DataMember]
        public string Variable { get; set; }

        [DataMember]
        public string Answer { get; set; }
    }

    public class AssignmentViewItem
    {
        [DataMember] public int Id { get; set; }

        [DataMember] public Guid ResponsibleId { get; set; }

        /// <summary>
        /// Name of the responsible person
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public string ResponsibleName { get; set; }

        /// <summary>
        /// Questionnaire Id 
        /// </summary>
        [DataMember]
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Quantity of submitted interviews for this assignment
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public int InterviewsCount { get; set; }

        /// <summary>
        /// Maximum allowed quantity of interviews that can be created from this assignment
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public int? Quantity { get; set; }

        /// <summary>
        /// Archived status to filter by. True or False
        /// </summary>
        [DataMember]
        public bool Archived { get; set; }

        /// <summary>
        /// Date (UTC) when assignment were created
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Last Update Date (UTC) of assignment
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public DateTime UpdatedAtUtc { get; set; }
        
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public bool? WebMode { get; set; }
        
        /// <summary>
        /// Represents date (UTC) when assignment was received by tablet
        /// </summary>
        [DataMember]
        public DateTime? ReceivedByTabletAtUtc { get; set; }

        /// <summary>
        /// Determines if interview from this assignment must record voice during interview (only on tablet)
        /// </summary>
        [DataMember]
        public bool IsAudioRecordingEnabled { get; set; }
    }

    public class AssignmentAssignRequest
    {
        [DataMember]
        [Required]
        public string Responsible { get; set; }
    }

    public class AssignmentsListView : BaseApiView
    {
        public AssignmentsListView(int page, int pageSize, int totalCount, string order)
        {
            this.Offset = page;
            this.TotalCount = totalCount;
            this.Limit = pageSize;
            this.Order = order;
        }

        public List<AssignmentViewItem> Assignments { get; set; }
    }

    public class AssignmentsListFilter
    {
        /// <summary>
        /// Filter result by custom search query
        /// </summary>
        public string SearchBy { get; set; }

        /// <summary>
        /// Questionnaire Id in format of `{QuestionnaireId}${Version}`
        /// </summary>
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Responsible user Id on name
        /// </summary>
        public string Responsible { get; set; }

        /// <summary>
        /// Supervisor id
        /// </summary>
        public Guid? SupervisorId { get; set; }

        /// <summary>
        /// Search for only archived assignments
        /// </summary>
        public bool ShowArchive { get; set; }

        /// <summary>
        /// Possible values are
        /// Id, ResponsibleName, InterviewsCount, Quantity, UpdatedAtUtc, CreatedAtUtc
        /// Followed by ordering direction "ASC" or "DESC"
        /// </summary>
        public string Order { get; set; }

        public int Offset { get; set; }
        public int Limit { get; set; }
    }

    public class CreateAssignmentResult
    {
        public AssignmentDetails Assignment { get; set; }
        public ImportDataVerificationState VerificationStatus { get; set; }
        public string WebInterviewLink { get; set; }
    }

    public class CreateAssignmentApiRequest
    {
        [DataMember]
        [Required]
        public string Responsible { get; set; }

        /// <summary>
        /// Maximum number of allowed to create assignments
        /// </summary>
        [DataMember]
        public int? Quantity { get; set; }

        /// <summary>
        /// QuestionnaireId for assignment
        /// </summary>
        [DataMember]
        [Required]
        public string QuestionnaireId { get; set; }

        [DataMember]
        [Required]
        public List<AssignmentIdentifyingDataItem> IdentifyingData { get; set; } =
            new List<AssignmentIdentifyingDataItem>();


        [DataMember] public string Email { get; set; }

        [DataMember] public string Password { get; set; }

        [DataMember] public bool WebMode { get; set; }

        [DataMember] public bool? IsAudioRecordingEnabled { get; set; }
        [DataMember] public string Comments { get; set; }
        
        /// <summary>
        /// List of protected variables
        /// </summary> 
        [DataMember]
        public List<string> ProtectedVariables { get; set; } = new List<string>();
    }
}
