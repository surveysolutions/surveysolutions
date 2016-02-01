using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public interface ITeamViewFactory
    {
        UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy);
        UsersView GetInterviewers(int pageSize, string searchBy, Guid supervisorId);
        UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false);
        UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId);
    }
}