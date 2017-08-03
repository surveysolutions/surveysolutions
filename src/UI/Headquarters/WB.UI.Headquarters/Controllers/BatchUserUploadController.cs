using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Threading;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class BatchUserUploadController : BaseController
    {
        private readonly IUserPreloadingService userPreloadingService;
        private readonly IUserBatchCreator userBatchCreator;
        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        public BatchUserUploadController(
            ICommandService commandService, 
            ILogger logger, 
            IUserPreloadingService userPreloadingService,
            IUserBatchCreator userBatchCreator) : 
            base(commandService, logger)
        {
            this.userPreloadingService = userPreloadingService;
            this.userBatchCreator = userBatchCreator;
        }

        public ActionResult UserBatchUploads()
        {
            this.ViewBag.ActivePage = MenuItem.UserBatchUpload;
            return this.View();
        }

        public ActionResult NewUserBatchUpload()
        {
            this.ViewBag.ActivePage = MenuItem.UserBatchUpload;

            return this.View(new UserBatchUploadModel
            {
                AvaliableDataColumnNames = this.userPreloadingService.GetAvaliableDataColumnNames()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult NewUserBatchUpload(UserBatchUploadModel model)
        {
            this.ViewBag.ActivePage = MenuItem.UserBatchUpload;

            if (!this.ModelState.IsValid)
                return this.View(model);

            var fileExtension = Path.GetExtension(model.File.FileName).ToLower();
            var extensionAccepted = fileExtension == ".txt" || fileExtension == ".tab";
            if (!extensionAccepted)
            {
                this.Error(string.Format(BatchUpload.UploadUsers_NotAllowedExtension, ".tab", ".txt"));
                return this.RedirectToAction(nameof(NewUserBatchUpload));
            }

            try
            {
                var preloadedDataId = this.userPreloadingService.CreateUserPreloadingProcess(model.File.InputStream,
                    model.File.FileName);
                
                return this.RedirectToAction(nameof(ImportUserDetails), new { id = preloadedDataId });
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return this.RedirectToAction(nameof(NewUserBatchUpload));
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
                return RedirectToAction(nameof(UserBatchUploads));
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

                return this.RedirectToAction(nameof(UserPreloadigVerificationDetails), new {id});
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return RedirectToAction(nameof(UserBatchUploads));
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
                return this.RedirectToAction(nameof(UserPreloadigVerificationDetails), new { id });
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
            return this.View(nameof(UserPreloadigVerificationDetails), null, id);
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

                Task.Run(() => this.userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync());

                return this.RedirectToAction(nameof(UserCreationProcessDetails), new {id});
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return RedirectToAction(nameof(UserBatchUploads));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult RefreshCreateUsers(string id)
        {
            try
            {
                Task.Run(() => this.userBatchCreator.CreateUsersFromReadyToBeCreatedQueueAsync());
                return this.RedirectToAction(nameof(UserCreationProcessDetails), new { id });
            }
            catch (Exception e)
            {
                Logger.Error("Error on user verification ", e);
                return RedirectToAction(nameof(UserBatchUploads));
            }
        }

        [ObserverNotAllowed]
        public ActionResult UserCreationProcessDetails(string id)
        {
            return this.View(nameof(UserCreationProcessDetails), null, id);
        }
    }
}