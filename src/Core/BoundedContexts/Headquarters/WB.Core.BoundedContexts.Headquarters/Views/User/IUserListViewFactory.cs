using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface IUserListViewFactory
    {
        UserListView GetUsersByRole(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, UserRoles role);

        InterviewersView GetInterviewers(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived,
            bool? hasDevice, Guid? supervisorId);

        SupervisorsView GetSupervisors(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived);
        UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false);
    }
}