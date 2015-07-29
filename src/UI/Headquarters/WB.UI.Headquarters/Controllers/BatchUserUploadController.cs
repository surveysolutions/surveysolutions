using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    public class BatchUserUploadController : BaseController
    {
        private readonly IUserPreloadingService userPreloadingService;
        public BatchUserUploadController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IUserPreloadingService userPreloadingService) : base(commandService, globalInfo, logger)
        {
            this.userPreloadingService = userPreloadingService;
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

            if (User.Identity.IsObserver())
            {
                this.Error("You cannot perform any operation in observer mode.");
                return this.View(model);
            }
            try
            {
                var preloadedDataId = this.userPreloadingService.CreateUserPreloadingProcess(model.File.InputStream,
                    model.File.FileName);

                return this.RedirectToAction("ImportUserDetails", new { id = preloadedDataId });
            }
            catch (UserPreloadingException e)
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
            return this.View(this.userPreloadingService.GetPreloadingProcesseDetails(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult VerifyUserPreloadig(string id)
        {
            this.userPreloadingService.EnqueueForValidation(id);
            return this.RedirectToAction("UserPreloadigVerificationDetails", new { id });
        }
        [ObserverNotAllowed]
        public ActionResult UserPreloadigVerificationDetails(string id)
        {
            return this.View("UserPreloadigVerificationDetails", null, id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public ActionResult CreateUsers(string id)
        {
            this.userPreloadingService.EnqueueForUserCreation(id);
            return this.RedirectToAction("UserCreationProcessDetails", new { id });
        }

        [ObserverNotAllowed]
        public ActionResult UserCreationProcessDetails(string id)
        {
            return this.View("UserCreationProcessDetails", null, id);
        }
    }
}