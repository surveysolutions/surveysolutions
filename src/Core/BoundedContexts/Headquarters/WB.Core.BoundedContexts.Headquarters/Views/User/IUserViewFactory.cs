using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface IUserViewFactory
    {
        UserListView GetUsersByRole(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, UserRoles role);

        InterviewersView GetInterviewers(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, int? apkBuildVersion, Guid? supervisorId, InterviewerFacet facet = InterviewerFacet.None);
        UsersView GetInterviewers(int pageSize, string searchBy, Guid? supervisorId, bool showLocked = false, bool? archived = false);

        SupervisorsView GetSupervisors(int pageIndex, int pageSize, string orderBy, string searchBy, bool? archived = null);
        UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false);

        UserView GetUser(UserViewInputModel input);

        ResponsibleView GetAllResponsibles(int pageSize, string searchBy, bool showLocked = false, bool showArchived = false);
        Task<ReportView> GetInterviewersReportAsync (int pageIndex, int pageSize, string getSortOrder, string searchValue, bool reqestArchived, int? interviewerApkVersion, Guid? supervisorId, InterviewerFacet reqestFacet);
    }
}