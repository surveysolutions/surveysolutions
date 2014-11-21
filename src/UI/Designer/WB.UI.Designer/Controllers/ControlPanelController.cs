using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : BaseController
    {
        private readonly IServiceLocator serviceLocator;

        public ControlPanelController(IServiceLocator serviceLocator, IMembershipUserService userHelper)
            : base(userHelper)
        {
            this.serviceLocator = serviceLocator;
        }

        /// <remarks>
        /// Getting dependency via service location ensures that parts of control panel not using it will always work.
        /// E.g. If Raven connection fails to be established then NConfig info still be available.
        /// </remarks>
        private IReadSideAdministrationService ReadSideAdministrationService
        {
            get { return this.serviceLocator.GetInstance<IReadSideAdministrationService>(); }
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult ReadLayer()
        {
            return this.RedirectToActionPermanent("ReadSide");
        }

        public ActionResult ReadSide()
        {
            return this.View(this.ReadSideAdministrationService.GetAllAvailableHandlers());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadSideStatus()
        {
            return this.ReadSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadSidePartially(string[] handlers, int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers, skipEvents);
            this.TempData["InProgress"] = true;
            this.TempData["CheckedHandlers"] = handlers;
            this.TempData["SkipEvents"] = skipEvents;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSidePartiallyForEventSources(string[] handlers, string eventSourceIds)
        {
            var sourceIds =
                eventSourceIds.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(e => Guid.Parse(e.Trim())).ToArray();
            this.ReadSideAdministrationService.RebuildViewForEventSourcesAsync(handlers, sourceIds);

            this.TempData["InProgress"] = true;
            this.TempData["CheckedHandlers"] = handlers;
            this.TempData["EventSources"] = eventSourceIds;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSide(int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync(skipEvents);
            this.TempData["InProgress"] = true;
            this.TempData["SkipEvents"] = skipEvents;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult StopReadSideRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadSide");
        }

        public ActionResult NCalcToCSharp()
        {
            return this.View();
        }

        public ActionResult ExpressionGeneration()
        {
            return this.View();
        }

        public ActionResult MakeAdmin()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult MakeAdmin(MakeAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = Membership.GetUser(model.UserName);
                if (account == null)
                {
                    this.Error(string.Format("Account '{0}' does not exists", model.UserName));
                }
                else
                {
                    if (model.MakeAdmin)
                    {
                        if (Roles.IsUserInRole(model.UserName, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' has administrator role", model.UserName));
                        }
                        else
                        {
                            Roles.AddUserToRole(account.ProviderUserKey.ToString(), UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully added", model.UserName));   
                        }
                    }
                    else
                    {
                        if (!Roles.IsUserInRole(model.UserName, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' is not in administrator role", model.UserName));
                        }
                        else
                        {
                            Roles.RemoveUserFromRole(model.UserName, UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully removed", model.UserName));    
                        }
                    }
                }
            }

            return this.View();
        }
    }
}