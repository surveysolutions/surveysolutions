using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Workspaces;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
    public class WorkspacesController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;

        public WorkspacesController(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser;
        }

        [ActivePage(MenuItem.Workspaces)]
        [Route("/Workspaces")]
        public IActionResult Index() => View(new WorkspacesModel
        {
            CanManage = authorizedUser.IsAdministrator,
            DataUrl = Url.Action("Index", "WorkspacesPublicApi"),
        });
    }
}
