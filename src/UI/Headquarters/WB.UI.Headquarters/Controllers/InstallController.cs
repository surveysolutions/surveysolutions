using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Headquarters.Controllers
{
    public class InstallController : BaseController
    {
        public InstallController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService, globalInfo, logger)
        {
        }

        public ActionResult Finish()
        {
            return View(new UserModel());
        }

        [HttpPost]
        public ActionResult Finish(UserModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.CommandService.Execute(new CreateUserCommand(publicKey: Guid.NewGuid(), userName: model.UserName,
                    password: SimpleHash.ComputeHash(model.Password), email: model.Email, isLockedBySupervisor: false,
                    isLockedByHQ: false, roles: new[] { UserRoles.Headquarter }, supervsor: null));
                    return this.RedirectToAction("LogOn", "Account");
                }
                catch (Exception ex)
                {
                    this.Logger.Fatal("Error when creating headquarters user", ex);
                }
            }

            return View(model);
        }
    }
}