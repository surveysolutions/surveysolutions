using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;

namespace WB.UI.Headquarters.Controllers
{
    [AllowAnonymous]
    public class ControlPanelController : Controller
    {
        public ActionResult NConfig()
        {
            return this.View();
        }
    }
}