using System;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class AssignmentsImportStatus
    {
        public string FileName { get; set; }
        public bool IsOwnerOfRunningProcess { get; set; }
        public string ResponsibleName { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public DateTime StartedDate { get; set; }
        public int AssignedToInterviewersCount { get; set; }
        public int AssignedToSupervisorsCount { get; set; }
        public long ProcessedCount { get; set; }
        public long InQueueCount { get; set; }
        public long VerifiedCount { get; set; }
        public long WithErrorsCount { get; set; }
        public long TotalCount { get; set; }
        public AssignmentsImportProcessStatus ProcessStatus { get; set; }
    }
}
