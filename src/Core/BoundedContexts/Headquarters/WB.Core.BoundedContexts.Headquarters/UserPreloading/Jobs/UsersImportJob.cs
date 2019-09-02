using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UsersImportJob : IJob
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;
        private readonly IUserImportService userImportService;
        private readonly ISystemLog systemLog;

        public UsersImportJob(IServiceLocator serviceLocator, ILogger logger, IUserImportService userImportService,
            ISystemLog systemLog)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.userImportService = userImportService;
            this.systemLog = systemLog;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.Info("User import job: Started");

            var sw = new Stopwatch();
            sw.Start();

            ImportUsers();

            sw.Stop();
            logger.Info($"User import job: Finished. Elapsed time: {sw.Elapsed}");
            return Task.CompletedTask;
        }

        private void ImportUsers()
        {
            try
            {
                var userToImport = this.userImportService.GetUserToImport();
                if (userToImport == null) return;

                do
                {
                    this.CreateUserOrUnarchiveAndUpdateAsync(userToImport).WaitAndUnwrapException();

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
            using (var userManager = serviceLocator.GetInstance<HqUserManager>())
            {
                var user = await userManager.FindByNameAsync(userToCreate.Login);
                if (user == null)
                {
                    Guid? supervisorId = null;

                    if (!string.IsNullOrEmpty(userToCreate.Supervisor))
                        supervisorId = (await userManager.FindByNameAsync(userToCreate.Supervisor))?.Id;

                    await userManager.CreateUserAsync(new HqUser
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
                    }, userToCreate.Password, userToCreate.UserRole);
                }
                else
                {
                    user.FullName = userToCreate.FullName;
                    user.Email = userToCreate.Email;
                    user.PhoneNumber = userToCreate.PhoneNumber;
                    user.IsArchived = false;

                    await userManager.UpdateUserAsync(user, userToCreate.Password);
                }
            }
        }
    }
}
