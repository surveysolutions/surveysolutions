using System;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize]
    public class AppController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IMembershipUserService userHelper;

        public AppController(
            ICommandService commandService,
            IMembershipUserService userHelper)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.userHelper = userHelper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Open(Guid id)
        {
            return Redirect("/Designer/app#/" + id.FormatGuid());
        }
    }
}