using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface ITeamViewFactory
    {
        UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy);
        UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId);
    }
}