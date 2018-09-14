using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UsersImportJob : IJob
    {
        private IServiceLocator serviceLocator;

        public UsersImportJob(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void Execute(IJobExecutionContext context)
        {
            ILogger logger = this.serviceLocator.GetInstance<ILoggerProvider>().GetFor<UsersImportJob>();

            logger.Info("User import job: Started");

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                serviceLocator.ExecuteActionInScope((serviceLocatorLocal) =>
                {
                    IUserImportService importUsersService = serviceLocatorLocal.GetInstance<IUserImportService>();

                    UserToImport userToImport = null;
                    do
                    {
                        userToImport = importUsersService.GetUserToImport();

                        if (userToImport == null) break;

                        this.CreateUserOrUnarchiveAndUpdateAsync(userToImport, serviceLocatorLocal).WaitAndUnwrapException();

                        importUsersService.RemoveImportedUser(userToImport);

                    } while (userToImport != null);
                });
            }
            catch (Exception ex)
            {
                logger.Error($"User import job: FAILED. Reason: {ex.Message} ", ex);
            }

            sw.Stop();
            logger.Info($"User import job: Finished. Elapsed time: {sw.Elapsed}");
        }

        private async Task CreateUserOrUnarchiveAndUpdateAsync(UserToImport userToCreate, IServiceLocator serviceLocator)
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
