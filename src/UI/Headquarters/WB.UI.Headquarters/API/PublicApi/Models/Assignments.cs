using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class AssignmentDetails : AssignmentViewItem
    {
        [DataMember]
        public List<AssignmentIdentifyingDataItem> IdentifyingData { get; set; }
    }

    public class AssignmentIdentifyingDataItem
    {
        public string Identity { get; set; }
        public string Variable { get; set; }
        public string Answer { get; set; }
    }

    public class AssignmentViewItem
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Guid ResponsibleId { get; set; }

        /// <summary>
        /// Name of the responsible person
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public string ResponsibleName { get; set; }

        /// <summary>
        /// Questionarire Id to filter by
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
        public bool Archived { get; set; } = false;

        /// <summary>
        /// Date when assignment were created
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Last Update Date of assignment
        /// Can be used for ordering
        /// </summary>
        [DataMember]
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class AssignmentAssignRequest
    {
        [DataMember]
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
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Responsible user Id on name
        /// </summary>
        public string Responsible { get; set; }
        public Guid? SupervisorId { get; set; }

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
        /// QuestionnaireId for assignemnt
        /// </summary>
        [DataMember]
        [Required]
        public string QuestionnaireId { get; set; }

        [DataMember]
        [Required]
        public List<AssignmentIdentifyingDataItem> IdentifyingData { get; set; } = new List<AssignmentIdentifyingDataItem>();
    }

}