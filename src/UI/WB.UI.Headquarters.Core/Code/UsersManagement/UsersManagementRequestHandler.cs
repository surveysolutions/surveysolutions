using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
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
        private readonly IAuthorizedUser authorizedUser;

        public UsersManagementRequestHandler(IUserRepository userRepository, IAuthorizedUser authorizedUser)
        {
            this.userRepository = userRepository;
            this.authorizedUser = authorizedUser;
        }

        public async Task<DataTableResponse<UserManagementListItem>> Handle(
            UsersManagementRequest request, CancellationToken cancellationToken = default)
        {
            var query = this.userRepository.Users
                .Where(u => 
                    u.Roles.All(r => r.Id != UserRoles.Administrator.ToUserId())
                );
            
            if (!authorizedUser.IsAdministrator)
            {
                var authorizedUserWorkspaces = authorizedUser.Workspaces;
                query = query.Where(u => u.Workspaces.Any(w => authorizedUserWorkspaces.Contains(w.Workspace.Name)));
            }

            if (authorizedUser.IsHeadquarter)
            {
                var hqAllowedRoles = new[] {UserRoles.Supervisor.ToUserId(), UserRoles.Interviewer.ToUserId()};
                query = query.Where(u => u.Roles.Any(r => hqAllowedRoles.Contains(r.Id)));
            }

            if (authorizedUser.IsSupervisor)
            {
                query = query.Where(u => 
                    u.Roles.Any(r => r.Id == UserRoles.Interviewer.ToUserId()
                    && u.Profile.SupervisorId == authorizedUser.Id));
            }

            if (authorizedUser.IsInterviewer)
                throw new ArgumentException("Interviewer can't see users");

            var recordsTotal = await query.CountAsync(cancellationToken);

            query = ApplyFiltering(request, query);

            var recordsFiltered = await query.CountAsync(cancellationToken);
            var sortOrder = request.GetSortOrder();

            var list = await query
                .Select(u => new UserManagementListItem(u.Id, u.UserName, u.Roles)
                {
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    UserId = u.Id,
                    CreationDate = u.CreationDate,
                    IsLocked = u.IsLockedByHeadquaters || u.IsLockedBySupervisor,
                    IsArchived = u.IsArchived
                })
                .OrderUsingSortExpression(sortOrder)
                .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userIds = list.Select(l => l.UserId).ToArray();

            var workspaces = (await this.userRepository.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    Workspaces = u.Workspaces.Select(w => new WorkspaceApiView
                    {
                        Disabled = w.Workspace.DisabledAtUtc != null,
                        Name = w.Workspace.Name,
                        DisplayName = w.Workspace.DisplayName
                    }).ToList()
                }).ToListAsync(cancellationToken))
                .ToDictionary(u => u.Id, u => u.Workspaces);

            foreach (var user in list)
            {
                if(workspaces.TryGetValue(user.UserId, out var ws))
                {
                    user.Workspaces = ws;
                }
            }

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
                var search = request.Search.Value.ToLower();
                query = query.Where(u => u.UserName.ToLower().Contains(search)
                                         || u.FullName.ToLower().Contains(search)
                                         || u.PhoneNumber.Contains(search)
                                         || u.Email.ToLower().Contains(search)
                                         || u.Workspaces.Any(w
                                             => (w.Workspace.Name.ToLower() + w.Workspace.DisplayName.ToLower()).Contains(search)));
            }

            if (request.Role != null)
            {
                var roleId = request.Role.Value.ToUserId();
                query = query.Where(u => u.Roles.Any(r => r.Id == roleId));
            }

            if (request.WorkspaceName != null)
                query = query.Where(u => u.Workspaces.Any(w => w.Workspace.Name == request.WorkspaceName));


            if (request.Filter != null)
            {
                query = request.Filter switch
                {
                    UserManagementFilter.WithMissingWorkspace => query.Where(u => u.Workspaces.Count == 0),
                    UserManagementFilter.WithDisabledWorkspaces => query.Where(u => u.Workspaces.Any() && u.Workspaces.All(w => w.Workspace.DisabledAtUtc != null)),
                    UserManagementFilter.Locked => query.Where(u => u.IsLockedByHeadquaters || u.IsLockedBySupervisor),
                    UserManagementFilter.Archived => query.Where(u => u.IsArchived),
                    _ => query
                };
            }
            
            return query;
        }
    }
}
