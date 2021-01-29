#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class CreateOrUnArchiveUserRequestHandler : IRequestHandler<CreateOrUnArchiveUserRequest, UserToImport?>
    {
        private readonly IUserRepository userRepository;
        private readonly UserManager<HqUser> userManager;
        private readonly IUserImportService userImportService;
        private readonly ISystemLog systemLog;

        public CreateOrUnArchiveUserRequestHandler(IUserRepository userRepository,
            UserManager<HqUser> userManager,
            IUserImportService importService, ISystemLog systemLog)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.userImportService = importService;
            this.systemLog = systemLog;
        }

        public async Task<UserToImport?> Handle(CreateOrUnArchiveUserRequest _, CancellationToken cancellationToken)
        {
            var userToImport = userImportService.GetUserToImport();
            await CreateUserOrUnarchiveAndUpdateAsync(userToImport);

            userImportService.RemoveImportedUser(userToImport);

            userToImport = userImportService.GetUserToImport();

            if (userToImport == null)
            {
                var completeStatus = userImportService.GetImportCompleteStatus();
                var status = userImportService.GetImportStatus();
                this.systemLog.UsersImported(completeStatus.SupervisorsCount, completeStatus.InterviewersCount, status.Responsible);
            }

            return userToImport;
        }

        private async Task CreateUserOrUnarchiveAndUpdateAsync(UserToImport userToCreate)
        {
            var user = await userManager.FindByNameAsync(userToCreate.Login);

            if (user == null)
            {
                Guid? supervisorId = null;

                if (!string.IsNullOrEmpty(userToCreate.Supervisor))
                    supervisorId = (await userManager.FindByNameAsync(userToCreate.Supervisor))?.Id;

                var role = userRepository.FindRole(userToCreate.UserRole.ToUserId());
                var hqUser = new HqUser
                {
                    Id = Guid.NewGuid(),
                    UserName = userToCreate.Login,
                    FullName = userToCreate.FullName,
                    Email = userToCreate.Email,
                    PhoneNumber = userToCreate.PhoneNumber,
                    Profile = supervisorId.HasValue
                        ? new HqUserProfile
                        {
                            SupervisorId = supervisorId
                        }
                        : null,
                };
                hqUser.Roles.Add(role);
                await userManager.CreateAsync(hqUser, userToCreate.Password);
            }
            else
            {
                user.FullName = userToCreate.FullName;
                user.Email = userToCreate.Email;
                user.PhoneNumber = userToCreate.PhoneNumber;
                user.IsArchived = false;
                user.ForceChangePassword = true;

                await userManager.UpdateAsync(user);
                string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, passwordResetToken, userToCreate.Password);
            }
        }
    }
}
