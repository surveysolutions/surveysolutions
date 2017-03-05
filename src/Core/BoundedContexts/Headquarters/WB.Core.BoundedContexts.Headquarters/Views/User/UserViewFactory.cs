using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserViewFactory : IUserViewFactory
    {
        protected readonly IUserRepository UserRepository;

        public UserViewFactory(IUserRepository UserRepository)
        {
            this.UserRepository = UserRepository;
        }

        public UserView GetUser(UserViewInputModel input)
        {
            Func<IQueryable<ApplicationUser>, ApplicationUser> query = users =>
            {
                if (input.PublicKey != null)
                    users = users.Where(x => x.Id == input.PublicKey);
                else if (!string.IsNullOrEmpty(input.UserName))
                    users = users.Where(x => x.UserName.ToLower() == input.UserName.ToLower());
                else if (!string.IsNullOrEmpty(input.UserEmail))
                    users = users.Where(x => x.Email.ToLower() == input.UserEmail.ToLower());
                else if (!string.IsNullOrEmpty(input.DeviceId))
                    users = users.Where(x => x.DeviceId == input.DeviceId);

                return users.FirstOrDefault();
            };

            var user = query.Invoke(this.UserRepository.Users);

            if (user == null) return null;

            UserLight supervisor = user.SupervisorId.HasValue
                ? new UserLight(user.SupervisorId.Value,
                    this.UserRepository.FindByIdAsync(user.SupervisorId.Value).Result.UserName)
                : null;

            return new UserView
            {
                CreationDate = user.CreationDate,
                UserName = user.UserName,
                Email = user.Email,
                IsLockedBySupervisor = user.IsLockedBySupervisor,
                IsLockedByHQ = user.IsLockedByHeadquaters,
                PublicKey = user.Id,
                Supervisor = supervisor,
                PersonName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsArchived = user.IsArchived,
                DeviceChangingHistory = new HashSet<DeviceInfo>(new[] { new DeviceInfo { DeviceId = user.DeviceId } }),
                Roles = new HashSet<UserRoles>(new[] {user.Roles.First().Role})
            };
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
                .Invoke(this.UserRepository.Users)
                .ToList();

            return new UserListView
            {
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = query.Invoke(this.UserRepository.Users).Count(),
                Items = filteredUsers.ToList()
            };
        }

        public UsersView GetInterviewers(int pageSize, string searchBy, Guid? supervisorId, bool onlyActive = false)
        {
            Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query = users =>
            {
                users = ApplyFilter(users, searchBy, UserRoles.Interviewer, onlyActive)
                    .Where(user => !user.IsLockedBySupervisor && !user.IsLockedByHeadquaters);

                if (supervisorId.HasValue)
                    users = users.Where(user => user.SupervisorId == supervisorId);

                return users;
            };

            var filteredUsers = query
                .PagedAndOrderedQuery(nameof(ApplicationUser.UserName), 1, pageSize)
                .Invoke(this.UserRepository.Users)
                .ToList()
                .Select(x => new UsersViewItem
                {
                    UserId = x.Id,
                    UserName = x.UserName
                });

            return new UsersView
            {
                TotalCountByQuery = query.Invoke(this.UserRepository.Users).Count(),
                Users = filteredUsers.ToList()
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
                .Invoke(this.UserRepository.Users)
                .ToList();

            return new InterviewersView
            {
                TotalCount = query.Invoke(this.UserRepository.Users).Count(),
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
                .Invoke(this.UserRepository.Users)
                .ToList()
                .Select(x => new UsersViewItem
                {
                    UserId = x.Id,
                    UserName = x.UserName
                });

            return new UsersView
            {
                TotalCountByQuery = query.Invoke(this.UserRepository.Users).Count(),
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

            var filteredUsers = query.PagedAndOrderedQuery(orderBy, pageIndex, pageSize).Invoke(this.UserRepository.Users).ToList();

            return new SupervisorsView
            {
                TotalCount = query.Invoke(this.UserRepository.Users).Count(),
                Items = filteredUsers
            };
        }

        private static IQueryable<ApplicationUser> ApplyFilter(IQueryable<ApplicationUser> _, string searchBy, UserRoles role, bool archived)
        {
            var selectedRoleId = ((byte)role).ToGuid();

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