using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface IUserViewFactory
    {
        UserListView GetUsersByRole(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, UserRoles role);
        InterviewersView GetInterviewers(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, InterviewerOptionFilter interviewerOptionFilter, int? apkBuildVersion, Guid? supervisorId);
        SupervisorsView GetSupervisors(int pageIndex, int pageSize, string orderBy, string searchBy, bool? archived = null);
        UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false);
        UsersView GetInterviewers(int pageSize, string searchBy, Guid? supervisorId, bool archived = false);
        UserView GetUser(UserViewInputModel input);
        ResponsibleView GetAllResponsibles(int pageSize, string searchBy, bool showLocked = false);
    }
}