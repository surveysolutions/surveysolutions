#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Responsible;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Utils;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    internal class UserViewFactory : IUserViewFactory
    {
        private readonly IUserRepository userRepository;
        private readonly IMemoryCache memoryCache;
        private IPlainStorageAccessor<DeviceSyncInfo> devicesSyncInfos;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public UserViewFactory(IUserRepository userRepository, 
            IMemoryCache memoryCache,
            IPlainStorageAccessor<DeviceSyncInfo> devicesSyncInfos,
            IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.userRepository = userRepository;
            this.memoryCache = memoryCache;
            this.devicesSyncInfos = devicesSyncInfos;
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        public UserViewLite? GetUser(Guid id)
        {
            return memoryCache.GetOrCreate(nameof(UserViewFactory) + ":" + id, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(1);

                    var user = GetUser(new UserViewInputModel(id));

                    return user == null
                        ? null
                        : new UserViewLite
                    {
                        Supervisor = user.Supervisor,
                        PublicKey = user.PublicKey,
                        Roles = user.Roles,
                        UserName = user.UserName
                    };
                });
        }

        public UserView? GetUser(UserViewInputModel input)
        {
            var query = this.userRepository.Users;

            if (input.PublicKey != null)
                query = query.Where(x => x.Id == input.PublicKey);
            else if (!string.IsNullOrEmpty(input.UserName))
                query = query.Where(x => x.UserName.ToLower() == input.UserName.ToLower());
            else if (!string.IsNullOrEmpty(input.UserEmail))
                query = query.Where(x => x.Email.ToLower() == input.UserEmail.ToLower());
            else if (!string.IsNullOrEmpty(input.DeviceId))
                query = query.Where(x => x.WorkspacesProfile.DeviceId == input.DeviceId);

            var dbUser =
                (from user in query
                select new
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
                    Roles = user.Roles,
                    SecurityStamp = user.SecurityStamp,
                    SupervisorId = user.Profile.SupervisorId
                }).FirstOrDefault();

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
                Roles = dbUser.Roles.Select(x => x.Id.ToUserRole()).ToImmutableHashSet(),
                SecurityStamp = dbUser.SecurityStamp,
                Supervisor = dbUser.SupervisorId.HasValue
                    ? new UserLight(dbUser.SupervisorId.Value, this.userRepository.Users.FirstOrDefault(x => x.Id == dbUser.SupervisorId)?.UserName)
                    : null
            };
        }

        public UserToVerify[] GetUsersByUserNames(string[] userNames)
        {
            var interviewerRoleId = UserRoles.Interviewer.ToUserId();
            var supervisorRoleId = UserRoles.Supervisor.ToUserId();
            var hqRoleId = UserRoles.Headquarter.ToUserId();

            return this.userRepository.Users
                .Where(x => userNames.Contains(x.UserName) && !x.IsArchived)
                .Select(x => new UserToVerify
                {
                    IsLocked = x.IsLockedByHeadquaters || x.IsLockedBySupervisor,
                    SupervisorId = x.Roles.Any(role => role.Id == supervisorRoleId) ? x.Id : x.Profile.SupervisorId,
                    InterviewerId = x.Roles.Any(role => role.Id == interviewerRoleId) ? x.Id : (Guid?)null,
                    HeadquartersId = x.Roles.Any(role => role.Id == hqRoleId) ? x.Id : (Guid?)null
                }).ToArray();
        }

        public UserListView GetUsersByRole(int pageIndex, int pageSize, string orderBy, string searchBy, bool? archived, UserRoles role, string? workspace = null)
        {
            var allUsers = ApplyFilter(this.userRepository.Users, searchBy, archived, workspace, role)
                .Select(x => new InterviewersItem
                {
                    UserId = x.Id,
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    IsArchived = x.IsArchived,
                    IsLockedBySupervisor = x.IsLockedBySupervisor,
                    IsLockedByHQ = x.IsLockedByHeadquaters,
                    UserName = x.UserName,
                    FullName = x.FullName,
                    DeviceId = x.WorkspacesProfile.DeviceId
                });

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(HqUser.UserName) : orderBy;

            var filteredUsers = allUsers
                .OrderUsingSortExpression(orderBy)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToList();

            return new UserListView
            {
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = allUsers.Count(),
                Items = filteredUsers.ToList()
            };
        }

        public UsersView GetInterviewers(int pageSize, string searchBy, Guid? supervisorId, bool showLocked = false, bool? archived = false)
        {
            var users = ApplyFilter(this.userRepository.Users, searchBy, archived, UserRoles.Interviewer)
                .Where(user => showLocked || (!user.IsLockedBySupervisor && !user.IsLockedByHeadquaters));

            if (supervisorId.HasValue)
                users = users.Where(user => user.Profile.SupervisorId == supervisorId);

            var filteredUsers = users
                .OrderBy(x => x.UserName)
                .Take(pageSize)
                .ToList()
                .Select(x => new UsersViewItem
                {
                    UserId = x.Id,
                    UserName = x.UserName,
                    IconClass = UserRoles.Interviewer.ToString().ToLower(),
                });

            var result = new UsersView
            {
                TotalCountByQuery = users.Count(),
                Users = filteredUsers.ToList()
            };

            return result;
        }

        public IEnumerable<InterviewerFullApiView> GetInterviewers(Guid supervisorId)
        {
            var repository = this.userRepository;

            Func<IQueryable<HqUser>, IQueryable<InterviewerFullApiView>> query = allUsers =>
            {
                var interviewers = ApplyFilter(allUsers, null, null, UserRoles.Interviewer);

                interviewers = ApplyFacetFilter(null, InterviewerFacet.None, interviewers);

                interviewers = AppySupervisorFilter(supervisorId, interviewers);

                return interviewers.Select(x => new InterviewerFullApiView
                {
                    Id = x.Id,
                    CreationDate = x.CreationDate,
                    Email = x.Email,
                    FullName = x.FullName,
                    UserName = x.UserName,
                    PhoneNumber = x.PhoneNumber,
                    PasswordHash = x.PasswordHash,
                    IsLockedByHeadquarters = x.IsLockedByHeadquaters,
                    IsLockedBySupervisor = x.IsLockedBySupervisor || x.IsArchived,
                    SecurityStamp = x.SecurityStamp
                });
            };

            var filteredUsers = query.Invoke(repository.Users).ToList();

            return filteredUsers;
        }

        public InterviewersView GetInterviewers(int pageIndex, int pageSize, string orderBy, string searchBy,
            bool archived, int? apkBuildVersion, Guid? supervisorId,
            InterviewerFacet facet = InterviewerFacet.None)
        {
            var repository = this.userRepository;

            var allUsers = repository.Users;
            
            var interviewers = ApplyFilter(allUsers, searchBy, archived, UserRoles.Interviewer);

            interviewers = ApplyFacetFilter(apkBuildVersion, facet, interviewers);

            interviewers = AppySupervisorFilter(supervisorId, interviewers);

            var query = interviewers.Select(x => new InterviewersItem
            {
                UserId = x.Id,
                CreationDate = x.CreationDate,
                Email = x.Email,
                IsLockedBySupervisor = x.IsLockedBySupervisor,
                IsLockedByHQ = x.IsLockedByHeadquaters,
                UserName = x.UserName,
                FullName = x.FullName,
                SupervisorId = x.Profile.SupervisorId,
                DeviceId = x.Profile.DeviceId,
                IsArchived = x.IsArchived,
                EnumeratorVersion = x.Profile.DeviceAppVersion,
                EnumeratorBuild = x.Profile.DeviceAppBuildVersion,
                LastLoginDate = x.LastLoginDate
            });
            

            orderBy = string.IsNullOrWhiteSpace(orderBy) ? nameof(HqUser.UserName) : orderBy;
            var filteredUsers = query
                .OrderUsingSortExpression(orderBy)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToList();

            var interviewersIds = filteredUsers.Select(x => x.UserId).ToArray();
            var supervisorIds = filteredUsers.Select(x => x.SupervisorId).ToArray();

            var deviceSyncInfos = this.devicesSyncInfos.Query(_ => _
                .Where(d => interviewersIds.Contains(d.InterviewerId) && d.Statistics != null)
                .GroupBy(d => d.InterviewerId)
                .Select(g => new
                {
                    InterviewerId = g.Key,
                    TrafficUsed = g.Sum(x => x.Statistics.TotalDownloadedBytes + x.Statistics.TotalUploadedBytes)
                }).ToList());
            var supervisors = this.userRepository.Users
                .Where(x => supervisorIds.Contains(x.Id))
                .Select(x => new {x.Id, x.UserName})
                .ToList();

            foreach (var interviewer in filteredUsers)
            {
                interviewer.TrafficUsed = deviceSyncInfos.FirstOrDefault(x => x.InterviewerId == interviewer.UserId)
                    ?.TrafficUsed;
                interviewer.SupervisorName = supervisors.FirstOrDefault(x => x.Id == interviewer.SupervisorId)?.UserName;
            }

            return new InterviewersView
            {
                Page = pageIndex,
                PageSize = pageSize,
                TotalCount = query.Count(),
                Items = filteredUsers.ToList()
            };
        }

        public Guid[] GetInterviewersIds(string searchBy, bool archived, int? apkBuildVersion, Guid? supervisorId, InterviewerFacet facet = InterviewerFacet.None)
        {
            var repository = this.userRepository;

            Func<IQueryable<HqUser>, IQueryable<Guid>> query = allUsers =>
            {
                var interviewers = ApplyFilter(allUsers, searchBy, archived, UserRoles.Interviewer);

                interviewers = ApplyFacetFilter(apkBuildVersion, facet, interviewers);

                interviewers = AppySupervisorFilter(supervisorId, interviewers);

                return interviewers
                    .OrderBy(x => x.UserName)
                    .Select(x => x.Id);
            };

            return query.Invoke(repository.Users).ToArray();
        }

        private static IQueryable<HqUser> AppySupervisorFilter(Guid? supervisorId, IQueryable<HqUser> interviewers)
        {
            if (supervisorId.HasValue)
                interviewers =
                    interviewers.Where(x => x.Profile.SupervisorId != null && x.Profile.SupervisorId == supervisorId);
            return interviewers;
        }

        private static IQueryable<HqUser> ApplyFacetFilter(int? apkBuildVersion, InterviewerFacet facet, IQueryable<HqUser> interviewers)
        {
            switch (facet)
            {
                case InterviewerFacet.NeverSynchonized:
                    interviewers = interviewers.Where(x => x.Profile.DeviceId == null);
                    break;
                case InterviewerFacet.OutdatedApp:
                    interviewers = interviewers.Where(x =>
                        x.Profile.DeviceAppBuildVersion.HasValue && x.Profile.DeviceAppBuildVersion < apkBuildVersion);
                    break;
                case InterviewerFacet.LowStorage:
                    interviewers = from i in interviewers
                                    where i.Profile.StorageFreeInBytes < InterviewerIssuesConstants.LowMemoryInBytesSize
                                    select i;
                    break;
                case InterviewerFacet.NoAssignmentsReceived:
                    interviewers = from i in interviewers
                                   let deviceSyncInfo = i.DeviceSyncInfos
                                   where !deviceSyncInfo.Any(s => s.Statistics.DownloadedQuestionnairesCount > 0)
                                   select i;
                    break;
                case InterviewerFacet.NeverUploaded:
                    interviewers = from i in interviewers
                                   let deviceSyncInfo = i.DeviceSyncInfos
                                   where !deviceSyncInfo.Any(s => s.Statistics.UploadedInterviewsCount > 0)
                                   select i;
                    break;
                case InterviewerFacet.TabletReassigned:
                    interviewers = from i in interviewers
                                   let deviceSyncInfo = i.DeviceSyncInfos
                                   where deviceSyncInfo.Select(s => s.DeviceId).Distinct().Count() > 1
                                   select i;
                    break;
            }
            return interviewers;
        }

        public ResponsibleView GetAllResponsibles(int pageSize, string searchBy, bool showLocked = false, bool showArchived = false)
        {
            Func<IQueryable<HqUser>, IQueryable<ResponsiblesViewItem>> query = users =>
            {
                bool? isArchivedShowed = showArchived ? (bool?)null : false;
                string searchByToLower = searchBy?.ToLower() ?? string.Empty;

                var responsible = ApplyFilter(users, searchBy, isArchivedShowed, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Headquarter)
                    .Where(user => showLocked || !user.IsLockedByHeadquaters && !user.IsLockedBySupervisor);

                return responsible.Select(x => new ResponsiblesViewItem
                {
                    InterviewerId = x.Profile.SupervisorId.HasValue ? x.Id : (Guid?)null,
                    SupervisorId = x.Profile.SupervisorId ?? x.Id,
                    UserName = x.UserName,
                    Rank = x.UserName.ToLower().StartsWith(searchByToLower) ? 1 : 0
                });
            };

            var orderByRankAndUserName = nameof(ResponsiblesViewItem.Rank) + " Desc," + nameof(ResponsiblesViewItem.UserName);
            var filteredUsers = query
                .PagedAndOrderedQuery(orderByRankAndUserName, 1, pageSize)
                .Invoke(this.userRepository.Users)
                .ToList();

            return new ResponsibleView
            {
                TotalCountByQuery = query.Invoke(this.userRepository.Users).Count(),
                Users = filteredUsers.ToList()
            };
        }

        public UsersView GetAllSupervisors(int pageSize, string searchBy, bool showLocked = false)
        {
            var users =
                ApplyFilter(this.userRepository.Users, searchBy, false, UserRoles.Supervisor)
                    .Where(user => showLocked || !user.IsLockedByHeadquaters);

            var filteredUsers = users
                .OrderBy(x => x.UserName)
                .Take(pageSize)
                .Select(x => new UsersViewItem
                {
                    UserId = x.Id,
                    UserName = x.UserName
                });

            return new UsersView
            {
                TotalCountByQuery = users.Count(),
                Users = filteredUsers.ToList()
            };
        }

        public SupervisorsView GetSupervisors(int pageIndex, int pageSize, string orderBy, string searchBy, bool? archived = null)
        {
            var allUsers = ApplyFilter(this.userRepository.Users, searchBy, archived, UserRoles.Supervisor)
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

            List<SupervisorsQueryItem> usersPage = allUsers
                .OrderUsingSortExpression(orderBy)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .ToList();

            var filteredUsers = usersPage.Select(x => new SupervisorsItem
            {
                IsLockedBySupervisor = x.IsLockedBySupervisor,
                IsLockedByHQ = x.IsLockedByHQ,
                CreationDate = x.CreationDate,
                Email = x.Email,
                UserId = x.UserId,
                UserName = x.UserName,
                IsArchived = x.IsArchived,
            }).ToList();

            return new SupervisorsView
            {
                TotalCount = allUsers.Count(),
                Items = filteredUsers
            };
        }

        private IQueryable<HqUser> ApplyFilter(IQueryable<HqUser> _, string? searchBy, bool? archived, params UserRoles[] role)
            => ApplyFilter(_, searchBy, archived, null, role);
        
        private IQueryable<HqUser> ApplyFilter(IQueryable<HqUser> _, string? searchBy, bool? archived, string? workspace = null, params UserRoles[] role)
        {
            var selectedRoleId = role.Select(x => x.ToUserId()).ToArray();
            
            var currentWorkspace = workspace ??
                                   workspaceContextAccessor.CurrentWorkspace()?.Name ?? 
                                   throw new MissingWorkspaceException();
            
            var allUsers = _.Where(x => x.Roles.Any(r => selectedRoleId.Contains(r.Id)))
                .Where(u => u.Workspaces.Any(w => w.Workspace.Name == currentWorkspace));

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
            public string UserName { get; set; } = string.Empty;
            public Guid UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public bool IsArchived { get; set; }
        }
    }
}
