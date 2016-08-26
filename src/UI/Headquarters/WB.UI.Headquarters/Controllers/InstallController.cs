using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    public class InstallController : BaseController
    {
        private readonly IPasswordHasher passwordHasher;

        public InstallController(ICommandService commandService,
                                 IGlobalInfoProvider globalInfo,
                                 ILogger logger,
                                 IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger)
        {
            this.passwordHasher = passwordHasher;
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

                //this.authentication.SignIn(model.UserName, true);

                this.supportedVersionProvider.RememberMinSupportedVersion();

                return this.RedirectToAction("Index", "Headquarters");
            }

            return View(model);
        }
    }
}