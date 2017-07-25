using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserBatchCreator : IUserBatchCreator
    {
        private readonly IUserPreloadingService userPreloadingService;
        private readonly ILogger logger;
        private HqUserManager userManager;

        private IPlainTransactionManager plainTransactionManager
            => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private bool IsWorking;

        public UserBatchCreator(IUserPreloadingService userPreloadingService, ILogger logger)
        {
            this.userPreloadingService = userPreloadingService;
            this.logger = logger;
        }

        public async Task CreateUsersFromReadyToBeCreatedQueueAsync()
        {
            if (IsWorking)
                return;

            IsWorking = true;

            try
            {
                while (IsWorking)
                {
                    string preloadingProcessIdToCreate = this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.DeQueuePreloadingProcessIdReadyToCreateUsers());

                    if (string.IsNullOrEmpty(preloadingProcessIdToCreate))
                        break;

                    var preloadingProcessDataToCreate =
                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToCreate).UserPrelodingData.ToList());
                    try
                    {

                        using (userManager = ServiceLocator.Current.GetInstance<HqUserManager>())
                        {
                            await this.CreateUsersFromPreloadedDataAsync(preloadingProcessDataToCreate, preloadingProcessIdToCreate);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"preloading process with id {preloadingProcessIdToCreate} finished with error", e);

                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () => userPreloadingService.FinishPreloadingProcessWithError(preloadingProcessIdToCreate,e.Message));
                        return;
                    }

                    this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => userPreloadingService.FinishPreloadingProcess(preloadingProcessIdToCreate));
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private async Task CreateUsersFromPreloadedDataAsync(List<UserPreloadingDataRecord> data, string id)
        {
            var usersToCreate = data.ToLookup(row => userPreloadingService.GetUserRoleFromDataRecord(row));

            foreach (var supervisorToCreate in usersToCreate[UserRoles.Supervisor])
            {
                await this.CreateUserOrUnarchiveAndUpdateAsync(supervisorToCreate, UserRoles.Supervisor);

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncrementCreatedUsersCount(id));
            }

            foreach (var interviewerToCreate in usersToCreate[UserRoles.Interviewer])
            {
                await CreateInterviewerOrUnarchiveAndUpdateAsync(interviewerToCreate);

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncrementCreatedUsersCount(id));
            }
        }

        private async Task CreateUserOrUnarchiveAndUpdateAsync(UserPreloadingDataRecord supervisorToCreate, UserRoles role, Guid? supervisorId = null)
        {
            var user = await this.userManager.FindByNameAsync(supervisorToCreate.Login);

            if (user == null)
            {
                await this.userManager.CreateUserAsync(new HqUser
                {
                    Id = Guid.NewGuid(),
                    UserName = supervisorToCreate.Login,
                    FullName = supervisorToCreate.FullName,
                    Email = supervisorToCreate.Email,
                    PhoneNumber = supervisorToCreate.PhoneNumber,
                    Profile = supervisorId.HasValue ? new HqUserProfile
                    {
                        SupervisorId = supervisorId
                    } : null,
                }, supervisorToCreate.Password, role);
            }
            else
            {
                var userRole = user.Roles.First().Role;

                if (userRole != role)
                    throw new UserPreloadingException($"user '{user.UserName}' is in role '{userRole}' " +
                                                      $"but must be in role {Enum.GetName(typeof(UserRoles), role)}");

                user.FullName = supervisorToCreate.FullName;
                user.Email = supervisorToCreate.Email;
                user.PhoneNumber = supervisorToCreate.PhoneNumber;
                user.IsArchived = false;

                await this.userManager.UpdateUserAsync(user, supervisorToCreate.Password);
            }
        }

        async Task CreateInterviewerOrUnarchiveAndUpdateAsync(UserPreloadingDataRecord interviewerToCreate)
        {
            var supervisor = await this.userManager.FindByNameAsync(interviewerToCreate.Supervisor);

            if (supervisor == null)
                throw new UserPreloadingException($"supervisor '{interviewerToCreate.Supervisor}' not found");

            await this.CreateUserOrUnarchiveAndUpdateAsync(interviewerToCreate, UserRoles.Interviewer, supervisor.Id);
        }
    }
}