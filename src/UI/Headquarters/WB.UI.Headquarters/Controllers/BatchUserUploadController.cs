using Microsoft.Practices.ServiceLocation;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Native.Threading;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class BatchUserUploadController : BaseController
    {
        private readonly IUserPreloadingService userPreloadingService;
        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        public BatchUserUploadController(
            ICommandService commandService, 
            IGlobalInfoProvider globalInfo, 
            ILogger logger, 
            IUserPreloadingService userPreloadingService) : 
            base(commandService, globalInfo, logger)
        {
            this.userPreloadingService = userPreloadingService;
        }

        public ActionResult UserBatchUploads()
        {
            this.ViewBag.ActivePage = MenuItem.UserBatchUpload;
            return this.View();
        }

        public ActionResult NewUserBatchUpload()
        {
            this.ViewBag.ActivePage = MenuItem.UserBatchUpload;
            return
                this.View(new UserBatchUploadModel()
                {
                    AvaliableDataColumnNames = userPreloadingService.GetAvaliableDataColumnNames()
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult NewUserBatchUpload(UserBatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.UserBatchUpload;

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                var preloadedDataId = this.userPreloadingService.CreateUserPreloadingProcess(model.File.InputStream,
                    model.File.FileName);
                
                return this.RedirectToAction("ImportUserDetails", new { id = preloadedDataId });
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return this.View(new UserBatchUploadModel()
                {
                    AvaliableDataColumnNames = userPreloadingService.GetAvaliableDataColumnNames()
                });
            }
        }
       
        [ObserverNotAllowed]
        public ActionResult ImportUserDetails(string id)
        {
            try
            {
                return this.View(this.userPreloadingService.GetPreloadingProcesseDetails(id));
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return RedirectToAction("UserBatchUploads");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [NoTransaction]
        public ActionResult VerifyUserPreloadig(string id)
        {
            try
            {
                plainTransactionManager.ExecuteInPlainTransaction(() =>
                    this.userPreloadingService.EnqueueForValidation(id));
                
                this.RunVerificationInSeparateTask();

                return this.RedirectToAction("UserPreloadigVerificationDetails", new {id});
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return RedirectToAction("UserBatchUploads");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult RefreshVerifyUsers(string id)
        {
            try
            {
                this.RunVerificationInSeparateTask();
                return this.RedirectToAction("UserPreloadigVerificationDetails", new { id });
            }
            catch (Exception e)
            {
                Logger.Error("Error on user verification ", e);
                return RedirectToAction("UserBatchUploads");
            }
        }

        private void RunVerificationInSeparateTask()
        {
            Task.Factory.StartNew(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                try
                {
                    ServiceLocator.Current.GetInstance<IUserPreloadingVerifier>()
                        .VerifyProcessFromReadyToBeVerifiedQueue();
                }
                catch (Exception exc)
                {
                    Logger.Error("Error on user verification ", exc);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            });
        }

        [ObserverNotAllowed]
        public ActionResult UserPreloadigVerificationDetails(string id)
        {
            return this.View("UserPreloadigVerificationDetails", null, id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [NoTransaction]
        public ActionResult CreateUsers(string id)
        {
            try
            {
                plainTransactionManager.ExecuteInPlainTransaction(() =>
                   this.userPreloadingService.EnqueueForUserCreation(id));
                
                this.RunCreationInSeparateTask();

                return this.RedirectToAction("UserCreationProcessDetails", new {id});
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return RedirectToAction("UserBatchUploads");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult RefreshCreateUsers(string id)
        {
            try
            {
                this.RunCreationInSeparateTask();
                return this.RedirectToAction("UserCreationProcessDetails", new { id });
            }
            catch (Exception e)
            {
                Logger.Error("Error on user verification ", e);
                return RedirectToAction("UserBatchUploads");
            }
        }

        private void RunCreationInSeparateTask()
        {
            Task.Factory.StartNew(() =>
            {
                ThreadMarkerManager.MarkCurrentThreadAsIsolated();
                try
                {
                    ServiceLocator.Current.GetInstance<IUserBatchCreator>().CreateUsersFromReadyToBeCreatedQueue();
                }
                catch (Exception exc)
                {
                    Logger.Error("Error on user verification ", exc);
                }
                finally
                {
                    ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                }
            });
        }

        [ObserverNotAllowed]
        public ActionResult UserCreationProcessDetails(string id)
        {
            return this.View("UserCreationProcessDetails", null, id);
        }
    }
}