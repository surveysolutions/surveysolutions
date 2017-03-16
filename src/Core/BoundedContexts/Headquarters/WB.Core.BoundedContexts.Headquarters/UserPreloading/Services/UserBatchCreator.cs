using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserBatchCreator : IUserBatchCreator
    {
        private readonly IUserPreloadingService userPreloadingService;
        private readonly ILogger logger;
        private readonly HqUserManager userManager;

        private IPlainTransactionManager plainTransactionManager
            => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private bool IsWorking;

        public UserBatchCreator(
            IUserPreloadingService userPreloadingService,
            ILogger logger,
            HqUserManager identityManager)
        {
            this.userPreloadingService = userPreloadingService;
            this.logger = logger;
            this.userManager = identityManager;
        }

        public void CreateUsersFromReadyToBeCreatedQueue()
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
                            () =>
                                userPreloadingService.GetPreloadingProcesseDetails(preloadingProcessIdToCreate)
                                    .UserPrelodingData.ToList());
                    try
                    {
                        this.CreateUsersFromPreloadedData(preloadingProcessDataToCreate, preloadingProcessIdToCreate);
                    }
                    catch (Exception e)
                    {
                        logger.Error(
                            string.Format("preloading process with id {0} finished with error",
                                preloadingProcessIdToCreate), e);

                        this.plainTransactionManager.ExecuteInPlainTransaction(
                            () =>
                                userPreloadingService.FinishPreloadingProcessWithError(preloadingProcessIdToCreate,
                                    e.Message));
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

        private void CreateUsersFromPreloadedData(IList<UserPreloadingDataRecord> data, string id)
        {
            var supervisorsToCreate =
                data.Where(row => userPreloadingService.GetUserRoleFromDataRecord(row) == UserRoles.Supervisor)
                    .ToArray();

            foreach (var supervisorToCreate in supervisorsToCreate)
            {
                this.CreateUserOrUnarchiveAndUpdate(supervisorToCreate, UserRoles.Supervisor);

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncrementCreatedUsersCount(id));
            }

            var interviewersToCreate =
                data.Where(row => userPreloadingService.GetUserRoleFromDataRecord(row) == UserRoles.Interviewer)
                    .ToArray();

            foreach (var interviewerToCreate in interviewersToCreate)
            {
                CreateInterviewerOrUnarchiveAndUpdate(interviewerToCreate);

                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.IncrementCreatedUsersCount(id));
            }
        }


        private void CreateUserOrUnarchiveAndUpdate(UserPreloadingDataRecord supervisorToCreate, UserRoles role, Guid? supervisorId = null)
        {
            var user = this.userManager.FindByName(supervisorToCreate.Login);

            if (user == null)
            {
                this.userManager.CreateUser(new HqUser
                {
                    Id = Guid.NewGuid(),
                    UserName = supervisorToCreate.Login,
                    FullName = supervisorToCreate.FullName,
                    Email = supervisorToCreate.Email,
                    PhoneNumber = supervisorToCreate.PhoneNumber,
                    SupervisorId = supervisorId,
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

                this.userManager.UpdateUser(user, supervisorToCreate.Password);
            }
        }

        void CreateInterviewerOrUnarchiveAndUpdate(UserPreloadingDataRecord interviewerToCreate)
        {
            var supervisor = this.userManager.FindByName(interviewerToCreate.Supervisor);
            if(supervisor == null)
                throw new UserPreloadingException($"supervisor '{interviewerToCreate.Supervisor}' not found");

            this.CreateUserOrUnarchiveAndUpdate(interviewerToCreate, UserRoles.Interviewer, supervisor.Id);
        }
    }
}