using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers;

[Route("/q")]
public class QController : Controller
{
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
}
