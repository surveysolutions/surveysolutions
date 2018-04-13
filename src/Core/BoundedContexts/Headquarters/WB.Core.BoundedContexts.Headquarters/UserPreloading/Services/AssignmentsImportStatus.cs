using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class AssignmentsImportStatus
    {
        public long AssignmentsInQueue { get; set; }
        public long VerifiedAssignments { get; set; }
        public long AssingmentsWithErrors { get; set; }
        public long TotalAssignments { get; set; }
        public string FileName { get; set; }
        public bool IsOwnerOfRunningProcess { get; set; }
        public string ResponsibleName { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public DateTime StartedDate { get; set; }
        public int AssignedToInterviewersCount { get; set; }
        public int AssignedToSupervisorsCount { get; set; }
        public long ProcessedCount { get; set; }
    }
}
