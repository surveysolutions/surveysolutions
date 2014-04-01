using System;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class SupervisorInterviewersInputModel : ListViewModelBase
    {
        public Guid SupervisorId { get; private set; }

        public SupervisorInterviewersInputModel(Guid supervisorId)
        {
            this.SupervisorId = supervisorId;
        }
    }
}