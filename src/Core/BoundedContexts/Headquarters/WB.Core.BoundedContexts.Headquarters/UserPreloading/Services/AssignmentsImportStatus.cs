using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    public class AssignmentsImportStatus
    {
        public bool IsInProgress { get; set; }
        public long AssignmentsInQueue { get; set; }
        public long TotalAssignmentsWithResponsible { get; set; }
        public string FileName { get; set; }
        public bool IsOwnerOfRunningProcess { get; set; }
        public string ResponsibleName { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public DateTime? StartedDate { get; set; }
    }
}
