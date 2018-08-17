using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UsersImportJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<UsersImportJob>();

        private IPlainTransactionManager transactionManager => ServiceLocator.Current
            .GetInstance<IPlainTransactionManager>();

        private IUserImportService importUsersService => ServiceLocator.Current
            .GetInstance<IUserImportService>();

        public void Execute(IJobExecutionContext context)
        {
            this.logger.Info("User import job: Started");

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                UserToImport userToImport = null;
                do
                {
                    userToImport = this.importUsersService.GetUserToImport();

                    if (userToImport == null) break;

                    this.CreateUserOrUnarchiveAndUpdateAsync(userToImport).WaitAndUnwrapException();

                    this.importUsersService.RemoveImportedUser(userToImport);

                } while (userToImport != null);
            }
            catch (Exception ex)
            {
                this.logger.Error($"User import job: FAILED. Reason: {ex.Message} ", ex);
            }

            sw.Stop();
            this.logger.Info($"User import job: Finished. Elapsed time: {sw.Elapsed}");
        }

        private async Task CreateUserOrUnarchiveAndUpdateAsync(UserToImport userToCreate)
        {
            using (var userManager = ServiceLocator.Current.GetInstance<HqUserManager>())
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
