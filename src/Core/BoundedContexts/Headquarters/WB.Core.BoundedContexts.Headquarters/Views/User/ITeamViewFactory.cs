using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface ITeamViewFactory
    {
        UsersView GetAssigneeSupervisors(int pageSize, string searchBy);
        UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy);
        UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId);
    }
}
