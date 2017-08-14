using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    internal class UserViewFactory : IUserViewFactory
    {
        private readonly IUserRepository userRepository;

        protected IUserRepository UserRepository => this.userRepository ?? ServiceLocator.Current.GetInstance<IUserRepository>();

        public UserViewFactory(IUserRepository UserRepository)
        {
            this.userRepository = UserRepository;
        }

        public UserViewFactory()
        {
        }

        public UserView GetUser(UserViewInputModel input)
        {
            var repository = this.UserRepository;
            var query = repository.Users.Select(user => new UserQueryItem
            {
                PublicKey = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PersonName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsArchived = user.IsArchived,
                IsLockedByHQ = user.IsLockedByHeadquaters,
                IsLockedBySupervisor = user.IsLockedBySupervisor,
                CreationDate = user.CreationDate,
                RoleId = user.Roles.FirstOrDefault().RoleId,
                DeviceId = user.Profile.DeviceId,
                SupervisorId = user.Profile.SupervisorId,
                SupervisorName = repository.Users.Select(x => new {Id = x.Id, Name = x.UserName})
                    .FirstOrDefault(x => user.Profile.SupervisorId == x.Id)
                    .Name
            });

            if (input.PublicKey != null)
                query = query.Where(x => x.PublicKey == input.PublicKey);
            else if (!string.IsNullOrEmpty(input.UserName))
                query = query.Where(x => x.UserName.ToLower() == input.UserName.ToLower());
            else if (!string.IsNullOrEmpty(input.UserEmail))
                query = query.Where(x => x.Email.ToLower() == input.UserEmail.ToLower());
            else if (!string.IsNullOrEmpty(input.DeviceId))
                query = query.Where(x => x.DeviceId == input.DeviceId);

            var dbUser = query.FirstOrDefault();
            if (dbUser == null) return null;

            return new UserView
            {
                PublicKey = dbUser.PublicKey,
                UserName = dbUser.UserName,
                Email = dbUser.Email,
                PersonName = dbUser.PersonName,
                PhoneNumber = dbUser.PhoneNumber,
                IsArchived = dbUser.IsArchived,
                IsLockedByHQ = dbUser.IsLockedByHQ,
                IsLockedBySupervisor = dbUser.IsLockedBySupervisor,
                CreationDate = dbUser.CreationDate,
                Supervisor = dbUser.SupervisorId.HasValue
                        ? new UserLight(dbUser.SupervisorId.Value, dbUser.SupervisorName)
                        : null,
                Roles = new HashSet<UserRoles>(new[] {dbUser.RoleId.ToUserRole()})
            };

        }

        public UserListView GetUsersByRole(int pageIndex, int pageSize, string orderBy, string searchBy, bool archived, UserRoles role)
        {
            Func<IQueryable<HqUser>, IQueryable<InterviewersItem>> query =
                allUsers => ApplyFilter(allUsers, searchBy, archived, role)
                    .Select(x => new InterviewersItem
                    {
                        UserId = x.Id,
                        CreationDate = x.CreationDate,
                        Email = x.Email,
                        IsArchived = x.IsArchived,
                        IsLockedBySupervisor = x.IsLockedBySupervisor,
                        IsLockedByHQ = x.IsLockedByHeadquaters,
                        UserName = x.UserName,
                        SupervisorName = allUsers.FirstOrDefault(pr => pr.Id == x.Profile.SupervisorId).UserName,
                        DeviceId = x.Profile.DeviceId
                    });

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(HqUser.UserName) : orderBy;

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

        public UsersView GetInterviewers(int pageSize, string searchBy, Guid? supervisorId, bool archived = false)
        {
            Func<IQueryable<HqUser>, IQueryable<HqUser>> query = users =>
            {
                users = ApplyFilter(users, searchBy, archived, UserRoles.Interviewer)
                    .Where(user => !user.IsLockedBySupervisor && !user.IsLockedByHeadquaters);

                if (supervisorId.HasValue)
                    users = users.Where(user => user.Profile.SupervisorId == supervisorId);

                return users;
            };

            var filteredUsers = query
                .PagedAndOrderedQuery(nameof(HqUser.UserName), 1, pageSize)
                .Invoke(this.UserRepository.Users)
                .ToList()
                .Select(x => new UsersViewItem
                {
                    UserId = x.Id,
                    UserName = x.UserName,
                    IconClass = UserRoles.Interviewer.ToString().ToLower()
                });

            var result = new UsersView
            {
                TotalCountByQuery = query.Invoke(this.UserRepository.Users).Count(),
                Users = filteredUsers.ToList()
            };

            return result;
        }

        public InterviewersView GetInterviewers(int pageIndex, int pageSize, string orderBy, string searchBy, 
            bool archived, InterviewerOptionFilter interviewerOptionFilter, int? apkBuildVersion, Guid? supervisorId)
        {
            Func<IQueryable<HqUser>, IQueryable<InterviewersItem>> query = allUsers =>
            {
                var interviewers = ApplyFilter(allUsers, searchBy, archived, UserRoles.Interviewer);

                switch (interviewerOptionFilter)
                {
                    case InterviewerOptionFilter.Any:
                        break;
                    case InterviewerOptionFilter.NotSynced:
                        interviewers = interviewers.Where(x => x.Profile.DeviceAppVersion == null);
                        break;
                    case InterviewerOptionFilter.UpToDate:
                        interviewers = interviewers.Where(x => apkBuildVersion.HasValue && apkBuildVersion <= x.Profile.DeviceAppBuildVersion);
                        break;
                    case InterviewerOptionFilter.Outdated:
                        interviewers = interviewers.Where(x => !(apkBuildVersion.HasValue && apkBuildVersion <= x.Profile.DeviceAppBuildVersion));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(interviewerOptionFilter), interviewerOptionFilter, null);
                }
                
                if (supervisorId.HasValue)
                    interviewers = interviewers.Where(x => x.Profile.SupervisorId != null && x.Profile.SupervisorId == supervisorId);

                return interviewers.Select(x => new InterviewersItem
                {
                    UserId = x.Id,
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    IsLockedBySupervisor = x.IsLockedBySupervisor,
                    IsLockedByHQ = x.IsLockedByHeadquaters,
                    UserName = x.UserName,
                    SupervisorName = allUsers.FirstOrDefault(pr => pr.Id == x.Profile.SupervisorId).UserName,
                    DeviceId = x.Profile.DeviceId,
                    IsArchived = x.IsArchived,
                    EnumeratorVersion = x.Profile.DeviceAppVersion,
                    EnumeratorBuild = x.Profile.DeviceAppBuildVersion
                });
            };

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(HqUser.UserName) : orderBy;

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

        public ResponsibleView GetAllResponsibles(int pageSize, string searchBy, bool showLocked = false)
        {
            Func<IQueryable<HqUser>, IQueryable<ResponsiblesViewItem>> query = users =>
            {
                var responsible = ApplyFilter(users, searchBy, false, UserRoles.Supervisor, UserRoles.Interviewer)
                    .Where(user => showLocked || !user.IsLockedByHeadquaters);

                return responsible.Select(x => new ResponsiblesViewItem
                {
                    InterviewerId = x.Profile.SupervisorId.HasValue ? x.Id : (Guid?)null,
                    SupervisorId = x.Profile.SupervisorId ?? x.Id,
                    UserName = x.UserName
                });
            };

            var filteredUsers = query
                .PagedAndOrderedQuery(nameof(HqUser.UserName), 1, pageSize)
                .Invoke(this.UserRepository.Users)
                .ToList();

            return new ResponsibleView
            {
                TotalCountByQuery = query.Invoke(this.UserRepository.Users).Count(),
                Users = filteredUsers.ToList()
            };
        }

        public UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false)
        {
            Func<IQueryable<HqUser>, IQueryable<HqUser>> query = users =>
                ApplyFilter(users, searchBy, false, UserRoles.Supervisor)
                    .Where(user => showLocked || !user.IsLockedByHeadquaters);

            var filteredUsers = query
                .PagedAndOrderedQuery(nameof(HqUser.UserName), 1, pageSize)
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

        public SupervisorsView GetSupervisors(int pageIndex, int pageSize, string orderBy, string searchBy, bool? archived = null)
        {
            Func<IQueryable<HqUser>, IQueryable<SupervisorsQueryItem>> query =
                allUsers => ApplyFilter(allUsers, searchBy, archived, UserRoles.Supervisor)
                    .Select(supervisor => new SupervisorsQueryItem
                    {
                        IsLockedBySupervisor = supervisor.IsLockedBySupervisor,
                        IsLockedByHQ = supervisor.IsLockedByHeadquaters,
                        CreationDate = supervisor.CreationDate,
                        Email = supervisor.Email,
                        UserId = supervisor.Id,
                        UserName = supervisor.UserName,
                        IsArchived = supervisor.IsArchived,
                    });

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(HqUser.UserName) : orderBy;

            List<SupervisorsQueryItem> usersPage = query.PagedAndOrderedQuery(orderBy, pageIndex, pageSize)
                .Invoke(this.UserRepository.Users)
                .ToList();

            var filteredUsers = usersPage.Select(x => new SupervisorsItem
            {
                IsLockedBySupervisor = x.IsLockedBySupervisor,
                IsLockedByHQ = x.IsLockedByHQ,
                CreationDate = x.CreationDate.FormatDateWithTime(),
                Email = x.Email,
                UserId = x.UserId,
                UserName = x.UserName,
                IsArchived = x.IsArchived,
            }).ToList();

            return new SupervisorsView
            {
                TotalCount = query.Invoke(this.UserRepository.Users).Count(),
                Items = filteredUsers
            };
        }

        private static IQueryable<HqUser> ApplyFilter(IQueryable<HqUser> _, string searchBy, bool? archived, params UserRoles[] role)
        {
            var selectedRoleId = role.Select(x => x.ToUserId());

            var allUsers = _.Where(x => selectedRoleId.Contains(x.Roles.FirstOrDefault().RoleId));

            if (archived.HasValue)
                allUsers = allUsers.Where(x => x.IsArchived == archived.Value);

            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                var searchByToLower = searchBy.ToLower();
                allUsers = allUsers.Where(x => x.UserName.ToLower().Contains(searchByToLower) || x.Email.ToLower().Contains(searchByToLower));
            }
            return allUsers;
        }

        public class SupervisorsQueryItem
        {
            public bool IsLockedBySupervisor { get; set; }
            public bool IsLockedByHQ { get; set; }
            public DateTime CreationDate { get; set; }
            public string UserName { get; set; }
            public Guid UserId { get; set; }
            public string Email { get; set; }
            public bool IsArchived { get; set; }
        }

        public class UserQueryItem
        {
            public DateTime CreationDate { get; set; }
            public string Email { get; set; }
            public bool IsLockedByHQ { get; set; }
            public bool IsArchived { get; set; }
            public bool IsLockedBySupervisor { get; set; }
            public Guid PublicKey { get; set; }
            public string UserName { get; set; }
            public Guid RoleId { get; set; }
            public string PersonName { get; set; }
            public string PhoneNumber { get; set; }
            public Guid? SupervisorId { get; set; }
            public string SupervisorName { get; set; }
            public string DeviceId { get; set; }
        }
    }
}