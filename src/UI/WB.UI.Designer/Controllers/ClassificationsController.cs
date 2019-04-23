using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers
{
    public class ClassificationsController : Controller
    {
        
        public IActionResult Index() => this.View();
    }
}
