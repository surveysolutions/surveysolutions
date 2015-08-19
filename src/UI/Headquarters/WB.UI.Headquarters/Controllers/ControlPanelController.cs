﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IPasswordHasher passwordHasher;
        private static string lastReexportMessage = "no reexport performed";
        private readonly IDataExportRepositoryWriter dataExportRepositoryWriter;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;

        private readonly ITransactionManagerProvider transactionManagerProvider;

        public ControlPanelController(
            IServiceLocator serviceLocator,
            IBrokenSyncPackagesStorage brokenSyncPackagesStorage,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IUserViewFactory userViewFactory,
            IPasswordHasher passwordHasher,
            ISettingsProvider settingsProvider,
            IDataExportRepositoryWriter dataExportRepositoryWriter,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries,
            ITransactionManagerProvider transactionManagerProvider)
            : base(serviceLocator, brokenSyncPackagesStorage, commandService, globalInfo, logger, settingsProvider, transactionManagerProvider)
        {
            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
            this.dataExportRepositoryWriter = dataExportRepositoryWriter;
            this.interviewSummaries = interviewSummaries;
            this.transactionManagerProvider = transactionManagerProvider;
        }

        public ActionResult ReexportInterviews()
        {
            return this.View();
        }

        public ActionResult StartReexportApprovedInterviews(int? skip)
        {
            new Task(() => this.ReexportApprovedInterviewsImpl(skip ?? 0)).Start();

            return this.RedirectToAction("ReexportInterviews");
        }

        private void ReexportApprovedInterviewsImpl(int skip)
        {
            int pageSize = 20;
            int count = 0;
            try
            {
                this.transactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
                count = this.GetInterviewIdsInCertainStatuses().Count();
            }
            finally
            {
                this.transactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
            }

            int errorsCount = 0;
            int processed = skip;
            List<Guid> notExportedInterviews = new List<Guid>();

            
            lastReexportMessage = string.Format("found {0} interviews", count);
            while (processed < count)
            {
                List<Guid> interviewIds;
                try
                {
                    this.transactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
                    interviewIds = this.GetInterviewIdsInCertainStatuses().Skip(processed).Take(pageSize).ToList();
                }
                finally
                {
                    this.transactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
                }

                foreach (var interviewId in interviewIds)
                {
                    try
                    {
                        this.transactionManagerProvider.GetTransactionManager().BeginCommandTransaction();
                        this.dataExportRepositoryWriter.AddExportedDataByInterview(interviewId);
                        this.transactionManagerProvider.GetTransactionManager().CommitCommandTransaction();
                    }
                    catch (Exception ex)
                    {
                        errorsCount++;
                        notExportedInterviews.Add(interviewId);
                        this.Logger.Error(ex.Message, ex);
                        this.transactionManagerProvider.GetTransactionManager().RollbackCommandTransaction();
                    }

                    processed++;
                    lastReexportMessage = string.Format("last processed interview index: {0} / {1}. Errors: {2}", processed, count, errorsCount);
                }
            }
            lastReexportMessage += Environment.NewLine;
            lastReexportMessage += string.Join(Environment.NewLine, notExportedInterviews);
        }

        private IQueryable<Guid> GetInterviewIdsInCertainStatuses()
        {
            return this.interviewSummaries.Query(_ =>
                _.Where(x 
                    => x.Status == InterviewStatus.ApprovedByHeadquarters 
                    || x.Status == InterviewStatus.ApprovedBySupervisor
                    || x.Status == InterviewStatus.RejectedByHeadquarters
                    || x.Status == InterviewStatus.RejectedBySupervisor)
                .OrderBy(x => x.SummaryId)
                .Select(x => x.InterviewId)).AsQueryable();
        }

        public string GetReexportStatus()
        {
            return lastReexportMessage;
        }

        public ActionResult CreateHeadquarters()
        {
            return this.View(new UserModel());
        }

        public ActionResult CreateAdmin()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateHeadquarters(UserModel model)
        {
            if (CreateUser(model, UserRoles.Headquarter))
                return this.RedirectToAction("LogOn", "Account");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(UserModel model)
        {
            if (CreateUser(model, UserRoles.Administrator))
                return this.RedirectToAction("LogOn", "Account");

            return View(model);
        }

        private bool CreateUser(UserModel model, UserRoles role)
        {
            if (ModelState.IsValid)
            {
                UserView userToCheck =
                    this.userViewFactory.Load(new UserViewInputModel(UserName: model.UserName, UserEmail: null));
                if (userToCheck == null)
                {
                    try
                    {
                        this.CommandService.Execute(new CreateUserCommand(publicKey: Guid.NewGuid(),
                            userName: model.UserName,
                            password: passwordHasher.Hash(model.Password), email: model.Email,
                            isLockedBySupervisor: false,
                            isLockedByHQ: false, roles: new[] { role }, supervsor: null,
                            personName: model.PersonName,
                            phoneNumber: model.PhoneNumber));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        var userErrorMessage = string.Format("Error when creating user {0} in role {1}", model.UserName, role);
                        this.Error(userErrorMessage);
                        this.Logger.Fatal(userErrorMessage, ex);
                    }
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }
            return false;
        }

        public ActionResult ResetPrivilegedUserPassword()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetUserPassword(UserModel model)
        {
            UserView userToCheck =
                this.userViewFactory.Load(new UserViewInputModel(UserName: model.UserName, UserEmail: null));
            if (userToCheck != null)
            {
                try
                {
                    this.CommandService.Execute(new ChangeUserCommand(publicKey: userToCheck.PublicKey,
                        email: userToCheck.Email, isLockedByHQ: userToCheck.IsLockedByHQ,
                        isLockedBySupervisor: userToCheck.IsLockedBySupervisor,
                        passwordHash: passwordHasher.Hash(model.Password), userId: Guid.Empty,
                        roles: userToCheck.Roles.ToArray(),
                        personName: userToCheck.PersonName, phoneNumber: userToCheck.PhoneNumber));

                    this.Success(string.Format("Password for user '{0}' successfully changed", userToCheck.UserName));
                }
                catch (Exception ex)
                {
                    var userErrorMessage = "Error when updating password for user";
                    this.Error(userErrorMessage);
                    this.Logger.Fatal(userErrorMessage, ex);
                }
            }
            else
            {
                this.Error(string.Format("User '{0}' does not exists", model.UserName));
            }

            return View(model);
        }
    }
}