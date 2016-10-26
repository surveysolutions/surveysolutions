using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface ITeamViewFactory
    {
        UsersView GetAllInterviewers(int pageSize, string searchBy, bool onlyActive = false);
        UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy);
        UsersView GetInterviewers(int pageSize, string searchBy, Guid supervisorId);
        UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId);
    }
}