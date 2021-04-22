using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IUserArchiveService
    {
        Task ArchiveSupervisorAndDependentInterviewersAsync(Guid userGuid);
        Task ArchiveUsersAsync(Guid[] guids);
        Task UnarchiveUsersAsync(Guid[] guids);
    }

    public class UserArchiveException : Exception
    {
        public UserArchiveException(string message) : base(message)
        {
        }
    }

    internal class UserArchiveService : IUserArchiveService
    {
        private readonly IUserRepository userRepository;
        private readonly ISystemLog auditLog;

        public UserArchiveService(IUserRepository userRepository,
            ISystemLog auditLog)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.auditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
        }

        public Task ArchiveSupervisorAndDependentInterviewersAsync(Guid supervisorId)
        {
            var supervisorAndDependentInterviewers = this.userRepository.Users.Where(
                user => user.Id == supervisorId || user.Workspaces.Any(w => w.Supervisor.Id == supervisorId))
                .ToList();

            foreach (var accountToArchive in supervisorAndDependentInterviewers)
            {
                this.ArchiveUser(accountToArchive);
            }

            return Task.CompletedTask;
        }

        public async Task ArchiveUsersAsync(Guid[] userIds)
        {
            var usersToArhive = this.userRepository.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (HqUser userToArchive in usersToArhive)
            {
                if (userToArchive.IsInRole(UserRoles.Interviewer))
                {
                    this.ArchiveUser(userToArchive);
                }
                else if (userToArchive.IsInRole(UserRoles.Supervisor))
                {
                    await this.ArchiveSupervisorAndDependentInterviewersAsync(userToArchive.Id);
                }
            }
        }

        public async Task UnarchiveUsersAsync(Guid[] userIds)
        {
            var usersToUnarhive = this.userRepository.Users.Where(user => userIds.Contains(user.Id)).ToList();
            foreach (var userToUnarchive in usersToUnarhive)
            {
                await this.UnarchiveUserAsync(userToUnarchive);
            }
        }

        private async Task UnarchiveUserAsync(HqUser userToUnarchive)
        {
            if (userToUnarchive.IsInRole(UserRoles.Interviewer))
            {
                var supervisorIds = userToUnarchive.Workspaces
                    .Where(w => w.Supervisor != null)
                    .Select(w => w.Supervisor.Id);
                foreach (var supervisorId in supervisorIds)
                {
                    var supervisor = await this.userRepository.FindByIdAsync(supervisorId);
                    if (supervisor.IsArchived)
                        throw new UserArchiveException(string.Format(HeadquarterUserCommandValidatorMessages.YouCantUnarchiveInterviewerUntilSupervisorIsArchivedFormat,
                            userToUnarchive.UserName));
                }
            }

            userToUnarchive.IsArchived = false;

            if (userToUnarchive.IsInRole(UserRoles.Supervisor))
                this.auditLog.SupervisorUnArchived(userToUnarchive.UserName);
            else if (userToUnarchive.IsInRole(UserRoles.Interviewer))
                this.auditLog.InterviewerUnArchived(userToUnarchive.UserName);
        }

        private void ArchiveUser(HqUser userToArchive)
        {
            userToArchive.IsArchived = true;

            if (userToArchive.IsInRole(UserRoles.Supervisor))
                this.auditLog.SupervisorArchived(userToArchive.UserName);
            else if (userToArchive.IsInRole(UserRoles.Interviewer))
                this.auditLog.InterviewerArchived(userToArchive.UserName);
        }

    }
}
