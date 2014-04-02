using System;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    /// <remarks>
    /// Do not reuse this class to get all interviewers.
    /// If such functionality will be needed, do following:
    /// 1. rename this class to SupervisorInterviewersInputModel,
    /// 2. create class AllInterviewersInputModel without SupervisorId property.
    /// </remarks>
    public class InterviewersInputModel : ListViewModelBase
    {
        /// <remarks>Do not make this type Guid? instead of Guid. Create a separate input module if needed.</remarks>
        public Guid SupervisorId { get; private set; }

        public InterviewersInputModel(Guid supervisorId)
        {
            this.SupervisorId = supervisorId;
        }
    }
}