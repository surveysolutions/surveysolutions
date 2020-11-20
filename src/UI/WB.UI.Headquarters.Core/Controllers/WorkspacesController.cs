using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.Models.Workspaces;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class WorkspacesController : Controller
    {
        public IActionResult Index() => View(new WorkspacesModel
        {
            DataUrl = Url.Action("Index", "WorkspacesPublicApi")
        });
    }
}
