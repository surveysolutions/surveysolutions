using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector
{
    public class InterviewerChangedArgs : EventArgs
    {
        public Guid PreviousResponsibleId { get; }
        public UserRoles PreviousResponsibleRole { get; }
        public Guid NewResponsibleId { get; }
        public UserRoles NewResponsibleRole { get; }

        public InterviewerChangedArgs(Guid previousResponsibleId, UserRoles previousResponsibleRole,Guid newResponsibleId, UserRoles newResponsibleRole)
        {
            PreviousResponsibleId = previousResponsibleId;
            PreviousResponsibleRole = previousResponsibleRole;
            NewResponsibleId = newResponsibleId;
            NewResponsibleRole = newResponsibleRole;
        }
    }
}
