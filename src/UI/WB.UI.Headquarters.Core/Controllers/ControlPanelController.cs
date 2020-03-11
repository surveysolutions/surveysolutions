using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models.Users;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ControlPanelController : Controller
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ITabletInformationService tabletInformationService;
        private readonly UserManager<HqUser> users;
        private readonly ICommandService commandService;
        private readonly ILogger<ControlPanelController> logger;

        public ControlPanelController(IUserViewFactory userViewFactory, 
            IAuthorizedUser authorizedUser,
            ITabletInformationService tabletInformationService, 
            UserManager<HqUser> users,
            ICommandService commandService,
            ILogger<ControlPanelController> logger)
        {
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
            this.tabletInformationService = tabletInformationService;
            this.users = users;
            this.commandService = commandService;
            this.logger = logger;
        }

        public ActionResult Index() => View();

        [ActivePage(MenuItem.Administration_TabletInfo)]
        public IActionResult TabletInfos() => View("Index");

        [AntiForgeryFilter]
        [ActivePage(MenuItem.Administration_CreateAdmin)]
        public IActionResult CreateAdmin()
        {
            return View("Index", new {});
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateUserModel model)
        {
        if (ModelState.IsValid)
        {
            var hqUser = new HqUser
            {
                UserName = model.UserName,
                Email = model.Email
            };
            var creationResult = await users.CreateAsync(hqUser, model.Password);
            if (creationResult.Succeeded)
            {
                await users.AddToRoleAsync(hqUser, "Administrator");
                return RedirectToAction("LogOn", "Account");
            }
            else
            {
                foreach (var error in creationResult.Errors)
                {
                    this.ModelState.AddModelError(
                        error.Code.StartsWith("Password")
                            ? nameof(CreateUserModel.Password)
                            : nameof(CreateUserModel.UserName),
                        error.Description);
                }
            }
        }

        return View("Index", new
        {
            Model = model,
            ModelState = this.ModelState.ErrorsToJsonResult()
        });
        }

        [HttpPost]
        public async Task<ActionResult> TabletInfos(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await using (var readStream = file.OpenReadStream())
                        await readStream.CopyToAsync(ms);

                    this.tabletInformationService.SaveTabletInformation(
                        content: ms.ToArray(),
                        androidId: @"manual-restore",
                        user: this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id)));
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Exception on tablet info uploading: {file?.Name}", exception);
            }

            return RedirectToAction("TabletInfos");
        }

        [ActivePage(MenuItem.Administration_Config)]
        public IActionResult Configuration() => View("Index");

        [ActivePage(MenuItem.Administration_Exceptions)]
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext);

        [ActivePage(MenuItem.Administration_AppUpdates)]
        public IActionResult AppUpdates() => View("Index");

        [ActivePage(MenuItem.Administration_InterviewPackages)]
        public IActionResult InterviewPackages() => this.View("Index");

        [HttpGet]
        [AntiForgeryFilter]
        [ActivePage(MenuItem.Administration_ChangePassword)]
        public IActionResult ResetPrivilegedUserPassword() => this.View("Index", new { });

        [HttpPost]
        [AntiForgeryFilter]
        [ValidateAntiForgeryToken]
        [ActivePage(MenuItem.Administration_ChangePassword)]
        public async Task<IActionResult> ResetPrivilegedUserPassword([FromForm] ChangePasswordByNameModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await users.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError(nameof(ChangePasswordByNameModel.UserName), Users.UserNotFound); 
                }
                else
                {
                    var resetToken = await users.GeneratePasswordResetTokenAsync(user);
                    var result = await users.ResetPasswordAsync(user, resetToken, model.Password);
                    if (result.Succeeded)
                    {
                        return View("Index", new
                        {
                            Model = model,
                            SucceededText = string.Format(Users.PasswordChanged, model.UserName),
                        });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            this.ModelState.AddModelError(nameof(ChangePasswordByNameModel.Password), error.Description);
                        }
                    }
                }
            }

            return View("Index", new
            {
                Model = model,
                ModelState = this.ModelState.ErrorsToJsonResult()
            });
        }

        [HttpGet]
        [ActivePage(MenuItem.Administration_ReevaluateInterview)]
        public IActionResult ReevaluateInterview() => this.View("Index");

        [HttpPost]
        public IActionResult ReevaluateInterview(Guid id)
        {
            try
            {
                this.commandService.Execute(new ReevaluateInterview(id, this.authorizedUser.Id));
                return this.Ok();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Exception while reevaluatng: {id}", exception);
                return this.UnprocessableEntity(exception);
            }
        }

        [HttpGet]
        public IActionResult RaiseException()
        {
            throw new ArgumentException("Test exception");
        }
    }
}
