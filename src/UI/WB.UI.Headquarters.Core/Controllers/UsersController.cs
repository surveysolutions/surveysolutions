using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Observer")]
    public class UsersController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;

        public UsersController(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser;
        }

        public class HeadquartersModel
        {
            public string DataUrl { get; set; }
            public string ImpersonateUrl { get; set; }
            public string EditUrl { get; set; }
            public string CreateUrl { get; set; }
            public bool ShowAddUser { get; set; }
            public bool ShowInstruction { get; set; }
            public bool ShowContextMenu { get; set; }
        }

        [Authorize(Roles = "Administrator, Observer")]
        [ActivePage(MenuItem.Headquarters)]
        [Route("/Headquarters")]
        public ActionResult Headquarters()
        {
            return this.View(new HeadquartersModel()
            {
                DataUrl = Url.Action("AllHeadquarters", "UsersApi"),
                ImpersonateUrl = authorizedUser.IsObserver
                    ? Url.Action("ObservePerson", "Account")
                    : null,
                EditUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Manage", "Account")
                    : null,
                CreateUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Create", "Account", new{ id = UserRoles.Headquarter })
                    : null,
                ShowAddUser = authorizedUser.IsAdministrator,
                ShowInstruction = !authorizedUser.IsObserving && !authorizedUser.IsObserver,
                ShowContextMenu = authorizedUser.IsObserver,
            });
        }
    }
}
