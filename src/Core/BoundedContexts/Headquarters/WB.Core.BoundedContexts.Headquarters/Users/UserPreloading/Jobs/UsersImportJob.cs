using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UsersImportJob : IJob
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;
        private readonly ISystemLog systemLog;

        public UsersImportJob(IServiceLocator serviceLocator, ILogger logger, ISystemLog systemLog)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.systemLog = systemLog;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.Info("User import job: Started");

            var sw = new Stopwatch();
            sw.Start();

            await ImportUsersAsync();

            sw.Stop();
            logger.Info($"User import job: Finished. Elapsed time: {sw.Elapsed}");
        }

        private async Task ImportUsersAsync()
        {
            try
            {
                var userImportService = this.serviceLocator.GetInstance<IUserImportService>();

                var userToImport = userImportService.GetUserToImport();
                if (userToImport == null) return;

                do
                {
                    await this.CreateUserOrUnarchiveAndUpdateAsync(userToImport);

                    userImportService.RemoveImportedUser(userToImport);

                    userToImport = userImportService.GetUserToImport();

                } while (userToImport != null);

                var completeStatus = userImportService.GetImportCompleteStatus();
                var status = userImportService.GetImportStatus();

                this.systemLog.UsersImported(completeStatus.SupervisorsCount, completeStatus.InterviewersCount, status.Responsible);
            }
            catch (Exception ex)
            {
                logger.Error($"User import job: FAILED. Reason: {ex.Message} ", ex);
            }
        }

        private async Task CreateUserOrUnarchiveAndUpdateAsync(UserToImport userToCreate)
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
