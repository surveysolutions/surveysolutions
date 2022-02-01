using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class ClassificationsController : Controller
    {
        
        public IActionResult Index() => this.View();
    }
}
