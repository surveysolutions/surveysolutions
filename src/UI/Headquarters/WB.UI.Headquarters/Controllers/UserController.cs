using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IFormsAuthentication authentication;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public UserController(
            IFormsAuthentication auth,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.authentication = auth;
            this.userViewFactory = userViewFactory;
        }

        [AllowAnonymous]
        public ActionResult CreateHeadquarters()
        {
            var supervisor = new HeadquartersModel(Guid.NewGuid(), "hq") ;

            return this.View(supervisor);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateHeadquarters(HeadquartersModel user)
        {
            if (this.ModelState.IsValid)
            {
                string userEmail = user.Name + "@example.com";

                UserView userToCheck = this.userViewFactory.Load(new UserViewInputModel(UserName: user.Name, UserEmail: null));
                if (userToCheck == null)
                {
                    this.CommandService.Execute(new CreateUserCommand(user.Id, user.Name, SimpleHash.ComputeHash(user.Name),
                                                                  userEmail, new[] { UserRoles.Headquarter },
                                                                  false, false, null));
                    if (this.GlobalInfo.GetCurrentUser() == null)
                        this.authentication.SignIn(user.Name, false);

                    return this.RedirectToAction("Index", "HQ");
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
                
            }

            return this.View(user);
        }
    }
}