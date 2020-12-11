using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Workspaces;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class WorkspacesController : Controller
    {
        [ActivePage(MenuItem.Workspaces)]
        public IActionResult Index() => View(new WorkspacesModel
        {
            DataUrl = Url.Action("Index", "WorkspacesPublicApi")
        });
    }
}
