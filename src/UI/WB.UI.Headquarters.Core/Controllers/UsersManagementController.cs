#nullable enable
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.UsersManagement;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(Roles = "Administrator")]
    public class UsersManagementController : Controller
    {
        private readonly IMediator mediator;

        public UsersManagementController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [ActivePage(MenuItem.UsersManagement)]
        public IActionResult Index() => View();

        public async Task<IActionResult> List(UsersManagementRequest request, CancellationToken cancellationToken)
        {
            var result = await this.mediator.Send(request, cancellationToken);
            return Json(result);
        }
    }
}
