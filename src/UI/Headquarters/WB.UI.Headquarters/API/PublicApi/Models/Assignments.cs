using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
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
        public Guid? QuestionId { get; set; }
        public string Variable { get; set; }
        public string Answer { get; set; }
    }

    public class AssignmentViewItem
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Guid ResponsibleId { get; set; }

        [DataMember]
        public string ResponsibleName { get; set; }

        [DataMember]
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Quantity of submitted interviews for this assignment
        /// </summary>
        [DataMember]
        public int InterviewsCount { get; set; }

        [DataMember]
        public int? Capacity { get; set; }

        [DataMember]
        public bool Archived { get; set; }

        [DataMember]
        public DateTime CreatedAtUtc { get; set; }

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
        public string Order { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateAssignmentResult
    {
        public AssignmentDetails Assignment { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
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
        public int? Capacity { get; set; }

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