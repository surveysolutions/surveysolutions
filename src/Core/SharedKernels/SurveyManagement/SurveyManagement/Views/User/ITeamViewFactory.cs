using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public interface ITeamViewFactory
    {
        UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy);
        UsersView GetSupervisorAndDependentInterviewers(int pageSize, string searchBy, Guid supervisorId);
        UsersView GetAllSupervisors(int pageSize, string searchBy);
        UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId);
    }
}