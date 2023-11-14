using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers;

[AllowAnonymous]
[Route("/q")]
public class QuestionnaireAppController : Controller
{
    [Route("{**catchAll}")]
    public ViewResult Index() => View("Vue");
}
