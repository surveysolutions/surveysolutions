using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    public class InstallController : BaseController
    {
        private readonly IPasswordHasher passwordHasher;

        public InstallController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, IPasswordHasher passwordHasher)
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
        public ActionResult Finish(UserModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.CommandService.Execute(
                        new CreateUserCommand(publicKey: Guid.NewGuid(), userName: model.UserName,
                                              password: passwordHasher.Hash(model.Password), 
                                              email: model.Email, isLockedBySupervisor: false,
                                              isLockedByHQ: false, roles: new[] { UserRoles.Administrator }, 
                                              supervsor: null,
                                              personName:model.PersonName,
                                              phoneNumber:model.PhoneNumber));

                    return this.RedirectToAction("LogOn", "Account");
                }
                catch (Exception ex)
                {
                    this.Logger.Fatal("Error when creating admin user", ex);
                }
            }

            return View(model);
        }
    }
}