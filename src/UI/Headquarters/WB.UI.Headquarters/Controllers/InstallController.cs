using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    public class InstallController : BaseController
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly ISupportedVersionProvider supportedVersionProvider;
        private readonly IFormsAuthentication authentication;

        public InstallController(ICommandService commandService,
                                 IGlobalInfoProvider globalInfo,
                                 ILogger logger,
                                 IPasswordHasher passwordHasher,
                                 ISupportedVersionProvider supportedVersionProvider,
                                 IFormsAuthentication authentication)
            : base(commandService, globalInfo, logger)
        {
            this.passwordHasher = passwordHasher;
            this.supportedVersionProvider = supportedVersionProvider;
            this.authentication = authentication;
        }

        public ActionResult Finish()
        {
            return View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventDoubleSubmit]
        public ActionResult Finish(UserModel model)
        {
            if (ModelState.IsValid)
            {
                this.CommandService.Execute(
                    new CreateUserCommand(publicKey: Guid.NewGuid(), userName: model.UserName,
                                            password: passwordHasher.Hash(model.Password), 
                                            email: model.Email, isLockedBySupervisor: false,
                                            isLockedByHQ: false, roles: new[] { UserRoles.Administrator }, 
                                            supervsor: null,
                                            personName:model.PersonName,
                                            phoneNumber:model.PhoneNumber));

                this.authentication.SignIn(model.UserName, true);

                this.supportedVersionProvider.RememberMinSupportedVersion();

                return this.RedirectToAction("Index", "Headquarters");
            }

            return View(model);
        }
    }
}