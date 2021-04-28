using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {

        public AdministrationController(){}

        [ActivePage(MenuItem.Administration_Diagnostics)]
        public IActionResult Diagnostics() => View("Index");
    }
}
