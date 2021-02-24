#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services
{
    public class CreateOrUnArchiveUserRequestHandler : IRequestHandler<CreateOrUnArchiveUserRequest, UserToImport?>
    {
        private readonly IUserRepository userRepository;
        private readonly UserManager<HqUser> userManager;
        private readonly IUserImportService userImportService;
        private readonly ISystemLog systemLog;
        private readonly IWorkspacesService workspacesService;

        public CreateOrUnArchiveUserRequestHandler(IUserRepository userRepository,
            UserManager<HqUser> userManager,
            IUserImportService importService, 
            ISystemLog systemLog,
            IWorkspacesService workspacesService)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.userImportService = importService;
            this.systemLog = systemLog;
            this.workspacesService = workspacesService;
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
                };
                hqUser.Roles.Add(role);
                
                await userManager.CreateAsync(hqUser, userToCreate.Password);
                var workspace = WorkspaceConstants.DefaultWorkspaceName; //TODO
                workspacesService.AddUserToWorkspace(hqUser, workspace, supervisorId);
            }
            else
            {
                user.FullName = userToCreate.FullName;
                user.Email = userToCreate.Email;
                user.PhoneNumber = userToCreate.PhoneNumber;
                user.IsArchived = false;

                await userManager.UpdateAsync(user);
                string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, passwordResetToken, userToCreate.Password);
            }
        }
    }
}
