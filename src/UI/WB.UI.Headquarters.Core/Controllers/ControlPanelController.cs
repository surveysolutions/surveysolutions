using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ControlPanelController : Controller
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ITabletInformationService tabletInformationService;

        public ControlPanelController(IUserViewFactory userViewFactory, 
            IAuthorizedUser authorizedUser,
            ITabletInformationService tabletInformationService)
        {
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
            this.tabletInformationService = tabletInformationService;
        }

        public ActionResult Index() => View();

        [ActivePage(MenuItem.Administration_TabletInfo)]
        public IActionResult TabletInfos() => View("Index");

        [HttpPost]
        public async Task<ActionResult> TabletInfos(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await using (var readStream = file.OpenReadStream())
                    await readStream.CopyToAsync(ms);

                this.tabletInformationService.SaveTabletInformation(
                    content: ms.ToArray(),
                    androidId: @"manual-restore",
                    user: this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id)));
            }

            return RedirectToAction("TabletInfos");
        }

        [ActivePage(MenuItem.Administration_Config)]
        public IActionResult Configuration() => View("Index");

        [ActivePage(MenuItem.Administration_Exceptions)]
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext);

        [ActivePage(MenuItem.Administration_AppUpdates)]
        public IActionResult AppUpdates() => View("Index");

        [ActivePage(MenuItem.Administration_InterviewPackages)]
        public IActionResult InterviewPackages() => this.View("Index");
    }
}
