#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.UsersManagement;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
    public class UsersManagementController : Controller
    {
        private readonly IMediator mediator;

        public UsersManagementController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [ActivePage(MenuItem.UsersManagement)]
        public IActionResult Index() => View(new
        {
            CreateUrl = Url.ActionAtWorkspace(Workspace.UsersWorkspace.AsContext(), "CreateUser", "UsersController"),
        });

        public async Task<DataTableResponse<UserManagementListItem>?> List(UsersManagementRequest request, CancellationToken cancellationToken)
        {
            DataTableResponse<UserManagementListItem>? result = await this.mediator.Send(request, cancellationToken);
            return result;
        }

        [ActivePage(MenuItem.UsersManagement)]
        public IActionResult CreateUser() => View();
    }
}
