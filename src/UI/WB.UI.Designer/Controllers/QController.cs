using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.UI.Designer.Controllers;

[Route("/q")]
public class QController : Controller
{
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
}
