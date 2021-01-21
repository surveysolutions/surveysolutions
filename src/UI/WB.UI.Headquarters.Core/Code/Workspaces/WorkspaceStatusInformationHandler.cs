using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class WorkspaceStatusInformationHandler :
        IRequestHandler<GetWorkspaceStatusInformation, WorkspaceStatusInformation>
    {
        private readonly IWorkspacesCache workspacesCache;
        private readonly IPlainStorageAccessor<WorkspacesUsers> workspaceUsersAccessor;
        private readonly IInScopeExecutor inScopeExecutor;

        public WorkspaceStatusInformationHandler(IPlainStorageAccessor<WorkspacesUsers> workspaceUsersAccessor,
            IInScopeExecutor inScopeExecutor, IWorkspacesCache workspacesCache)
        {
            this.workspaceUsersAccessor = workspaceUsersAccessor;
            this.inScopeExecutor = inScopeExecutor;
            this.workspacesCache = workspacesCache;
        }

        public async Task<WorkspaceStatusInformation> Handle(GetWorkspaceStatusInformation request,
            CancellationToken cancellationToken)
        {
            var workspace = workspacesCache.AllWorkspaces().FirstOrDefault(w => w.Name == request.WorkspaceName)
                            ?? throw new MissingWorkspaceException(
                                "Cannot find workspace with name: " + request.WorkspaceName);
            
            var interviewers = await this.workspaceUsersAccessor.Query(_ => _
                .Where(u => u.Workspace.Name == workspace.Name)
                .Where(u => u.User.Roles.Any(r => r.Id == UserRoles.Interviewer.ToUserId())
                ).CountAsync(cancellationToken: cancellationToken));

            var supervisors = await this.workspaceUsersAccessor.Query(_ => _
                .Where(u => u.Workspace.Name == workspace.Name)
                .Where(u => u.User.Roles.Any(r => r.Id == UserRoles.Supervisor.ToUserId())
                ).CountAsync(cancellationToken: cancellationToken));
            
            var response = new WorkspaceStatusInformation
            {
                WorkspaceName = workspace.Name,
                WorkspaceDisplayName = workspace.DisplayName,
                InterviewersCount = interviewers,
                SupervisorsCount = supervisors,
            };

            inScopeExecutor.Execute(s =>
            {
                var questionnairesAccessor = s.GetInstance<IQuestionnaireBrowseViewFactory>();
                var questionnaires = questionnairesAccessor.GetAllQuestionnaireIdentities();
                response.ExistingQuestionnairesCount = questionnaires.Count();
                response.CanBeDeleted = workspace.Name != WorkspaceConstants.DefaultWorkspaceName
                                        && response.ExistingQuestionnairesCount == 0;

                var mapBrowseViewFactory = s.GetInstance<IPlainStorageAccessor<MapBrowseItem>>();
                response.MapsCount = mapBrowseViewFactory.Query(_ => _.Count());
            }, request.WorkspaceName);

            return response;
        }
    }
}
