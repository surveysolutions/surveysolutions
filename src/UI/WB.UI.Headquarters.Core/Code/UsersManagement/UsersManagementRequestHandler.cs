using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Utils;
using WB.UI.Headquarters.Models.Api;
using WorkspaceApiView = WB.Core.SharedKernels.DataCollection.WebApi.WorkspaceApiView;

namespace WB.UI.Headquarters.Code.UsersManagement
{
    public class UsersManagementRequestHandler :
        IRequestHandler<UsersManagementRequest, DataTableResponse<UserManagementListItem>>
    {
        private readonly IUserRepository userRepository;

        public UsersManagementRequestHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<DataTableResponse<UserManagementListItem>> Handle(
            UsersManagementRequest request, CancellationToken cancellationToken = default)
        {
            var selectedRoleId = new[] { UserRoles.ApiUser,
                UserRoles.Headquarter }.Select(x => x.ToUserId()).ToArray();

            var query = this.userRepository.Users
                .Where(x => x.Roles.Any(r => selectedRoleId.Contains(r.Id)));

            var recordsTotal = await query.CountAsync(cancellationToken);

            query = ApplyFiltering(request, query);

            var recordsFiltered = await query.CountAsync(cancellationToken);
            var sortOrder = request.GetSortOrder();

            var list = await query
                .OrderUsingSortExpression(sortOrder)
                .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize)
                .Select(u => new UserManagementListItem
                {
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    UserId = u.Id,
                    CreationDate = u.CreationDate,
                    Workspaces = u.Workspaces.Select(w => new WorkspaceApiView
                    {
                        Name = w.Workspace.Name,
                        DisplayName = w.Workspace.DisplayName
                    }).ToList(),
                    IsLocked = u.IsLockedByHeadquaters || u.IsLockedBySupervisor,
                    IsArchived = u.IsArchived
                }).ToListAsync(cancellationToken);

            return new DataTableResponse<UserManagementListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = list
            };
        }

        private static IQueryable<HqUser> ApplyFiltering(UsersManagementRequest request, IQueryable<HqUser> query)
        {
            if (request.Search?.Value != null)
            {
                query = query.Where(u => u.UserName.Contains(request.Search.Value)
                                         || u.FullName.Contains(request.Search.Value)
                                         || u.Workspaces.Any(w
                                             => (w.Workspace.Name + w.Workspace.DisplayName).Contains(request.Search.Value)));
            }

            if (request.Role != null)
            {
                var roleId = request.Role.Value.ToUserId();
                query = query.Where(u => u.Roles.Any(r => r.Id == roleId));
            }

            if (!request.ShowArchived)
            {
                query = query.Where(u => u.IsArchived == false);
            }

            if (!request.ShowLocked)
            {
                query = query.Where(u => !u.IsLockedByHeadquaters && !u.IsLockedBySupervisor);
            }

            if (request.WorkspaceName != null && request.MissingWorkspace == false)
            {
                query = query.Where(u => u.Workspaces.Any(w => w.Workspace.Name == request.WorkspaceName));
            }
            else if (request.MissingWorkspace)
            {
                query = query.Where(u => u.Workspaces.Count == 0);
            }

            return query;
        }
    }
}
