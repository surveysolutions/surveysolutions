using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserListViewFactory : IUserListViewFactory
    {
        protected readonly IIdentityManager identityManager;

        public UserListViewFactory(IIdentityManager identityManager)
        {
            this.identityManager = identityManager;
        }

        public UserListView GetUsersByRole(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, UserRoles role)
        {
            Func<IQueryable<ApplicationUser>, IQueryable<InterviewersItem>> query =
                allUsers => ApplyFilter(allUsers, searchBy, role, archived)
                    .Select(x => new InterviewersItem
                    {
                        UserId = x.Id,
                        CreationDate = x.CreationDate,
                        Email = x.Email,
                        IsLockedBySupervisor = x.IsLockedBySupervisor,
                        IsLockedByHQ = x.IsLockedByHeadquaters,
                        UserName = x.UserName,
                        SupervisorName = allUsers.FirstOrDefault(pr => pr.Id == x.SupervisorId).UserName,
                        DeviceId = x.DeviceId
                    });

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(ApplicationUser.UserName) : orderBy;

            var filteredUsers = query
                .PagedAndOrderedQuery(orderBy, pageIndex, pageSize)
                .Invoke(this.identityManager.Users)
                .ToList();

            return new UserListView
            {
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = query.Invoke(this.identityManager.Users).Count(),
                Items = filteredUsers.ToList()
            };
        }

        public InterviewersView GetInterviewers(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, bool? hasDevice, Guid? supervisorId)
        {
            Func<IQueryable<ApplicationUser>, IQueryable<InterviewersItem>> query = allUsers =>
            {
                 var interviewers = ApplyFilter(allUsers, searchBy, UserRoles.Interviewer, archived);

                if (hasDevice.HasValue)
                    interviewers = interviewers.Where(x => (x.DeviceId != null) == hasDevice.Value);

                if (supervisorId.HasValue)
                    interviewers = interviewers.Where(x => x.SupervisorId != null && x.SupervisorId == supervisorId);

                return interviewers.Select(x => new InterviewersItem
                {
                    UserId = x.Id,
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    IsLockedBySupervisor = x.IsLockedBySupervisor,
                    IsLockedByHQ = x.IsLockedByHeadquaters,
                    UserName = x.UserName,
                    SupervisorName = allUsers.FirstOrDefault(pr => pr.Id == x.SupervisorId).UserName,
                    DeviceId = x.DeviceId
                });
            };

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(ApplicationUser.UserName) : orderBy;

            var filteredUsers = query
                .PagedAndOrderedQuery(orderBy, pageIndex, pageSize)
                .Invoke(this.identityManager.Users)
                .ToList();

            return new InterviewersView
            {
                TotalCount = query.Invoke(this.identityManager.Users).Count(),
                Items = filteredUsers.ToList()
            };
        }

        public UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false)
        {
            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query = users =>
                ApplyFilter(users, searchBy, UserRoles.Supervisor, false)
                    .Where(user => showLocked || !user.IsLockedByHeadquaters);
            
            var filteredUsers = query
                .PagedAndOrderedQuery(nameof(ApplicationUser.UserName), 1, pageSize)
                .Invoke(this.identityManager.Users)
                .ToList()
                .Select(x => new UsersViewItem
                {
                    UserId = x.Id,
                    UserName = x.UserName
                });

            return new UsersView
            {
                TotalCountByQuery = query.Invoke(this.identityManager.Users).Count(),
                Users = filteredUsers.ToList()
            };
        }

        public SupervisorsView GetSupervisors(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived)
        {
            Func<IQueryable<ApplicationUser>, IQueryable<SupervisorsItem>> query =
                allUsers => ApplyFilter(allUsers, searchBy, UserRoles.Supervisor, archived)
                    .Select(supervisor => new SupervisorsItem
                    {
                        UserId = supervisor.Id,
                        CreationDate = supervisor.CreationDate,
                        Email = supervisor.Email,
                        IsLockedBySupervisor = supervisor.IsLockedBySupervisor,
                        IsLockedByHQ = supervisor.IsLockedByHeadquaters,
                        UserName = supervisor.UserName,
                        InterviewersCount = allUsers.Count(pr => pr.SupervisorId == supervisor.Id && pr.IsArchived == false),
                        NotConnectedToDeviceInterviewersCount = allUsers.Count(pr => pr.SupervisorId == supervisor.Id && pr.DeviceId == null && pr.IsArchived == false)
                    });

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(ApplicationUser.UserName) : orderBy;

            var filteredUsers = query.PagedAndOrderedQuery(orderBy, pageIndex, pageSize).Invoke(this.identityManager.Users).ToList();

            return new SupervisorsView
            {
                TotalCount = query.Invoke(this.identityManager.Users).Count(),
                Items = filteredUsers
            };
        }

        private static IQueryable<ApplicationUser> ApplyFilter(IQueryable<ApplicationUser> _, string searchBy, UserRoles role, bool archived)
        {
            var selectedRoleId = ((byte) role).ToGuid();

            var allUsers = _.Where(x => x.IsArchived == archived && x.Roles.FirstOrDefault().RoleId == selectedRoleId);

            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                var searchByToLower = searchBy.ToLower();
                allUsers = allUsers.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
            }
            return allUsers;
        }
    }
}