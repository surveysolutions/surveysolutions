using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.Resources;

namespace WB.UI.Shared.Web.Controllers
{
    public class UnderConstructionController : Controller
    {
        public UnderConstructionInfo underConstructionInfo;

        public UnderConstructionController(UnderConstructionInfo underConstructionInfo)
        {
            this.underConstructionInfo = underConstructionInfo;
        }

        public class UnderConstructionModel
        {
            public string Title { get; set; }
            public string MainMessage { get; set; }
            public string SubMessage { get; set; }
        }
        
        public ActionResult Index()
        {
            var status = underConstructionInfo;

            if (status.Status == UnderConstructionStatus.Finished)
            {
                return Redirect(Url.Content("~/"));
            }

            var isError = status.Status == UnderConstructionStatus.Error;

            var model = new UnderConstructionModel()
            {
                Title = UnderConstruction.UnderConstructionTitle,
                MainMessage = isError ? status.Message : UnderConstruction.ServerInitializing,
                SubMessage = isError ? null : status.Message
            };

            return View(model);
        }
    }
}
