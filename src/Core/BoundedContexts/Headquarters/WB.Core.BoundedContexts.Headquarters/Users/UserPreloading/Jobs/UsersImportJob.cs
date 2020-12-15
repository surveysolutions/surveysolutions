using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;

// do not change this namespace since it breaks quartz when it is stored in database
namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UsersImportJob : IJob
    {
        private readonly IInScopeExecutor inScopeExecutor;
        private readonly ILogger<UsersImportJob> logger;
        private readonly IUserImportService userImportService;
        private readonly ISystemLog systemLog;

        public UsersImportJob(IInScopeExecutor inScopeExecutor, 
            ILogger<UsersImportJob> logger, 
            IUserImportService userImportService,
            ISystemLog systemLog)
        {
            this.inScopeExecutor = inScopeExecutor;
            this.logger = logger;
            this.userImportService = userImportService;
            this.systemLog = systemLog;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sw = new Stopwatch();
            sw.Start();

            await ImportUsersAsync();

            sw.Stop();
            logger.LogInformation("User import job: Finished. Elapsed time: {elapsed}", sw.Elapsed);
        }

        private async Task ImportUsersAsync()
        {
            try
            {
                var userToImport = userImportService.GetUserToImport();
                if (userToImport == null) return;

                do
                {
                    await this.inScopeExecutor.ExecuteAsync(async (serviceLocator) =>
                    {
                        await CreateUserOrUnarchiveAndUpdateAsync(userToImport, serviceLocator);

                        userImportService.RemoveImportedUser(userToImport);

                        userToImport = userImportService.GetUserToImport();
                    });
                } while (userToImport != null);

                var completeStatus = userImportService.GetImportCompleteStatus();
                var status = userImportService.GetImportStatus();

                this.systemLog.UsersImported(completeStatus.SupervisorsCount, completeStatus.InterviewersCount, status.Responsible);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "User import job: FAILED");
            }
        }

        private static async Task CreateUserOrUnarchiveAndUpdateAsync(UserToImport userToCreate, IServiceLocator serviceLocator)
        {
            var userManager = serviceLocator.GetInstance<UserManager<HqUser>>();
            var userRepository = serviceLocator.GetInstance<IUserRepository>();
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

                await userManager.UpdateAsync(user);
                string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, passwordResetToken, userToCreate.Password);
            }
        }
    }
}
