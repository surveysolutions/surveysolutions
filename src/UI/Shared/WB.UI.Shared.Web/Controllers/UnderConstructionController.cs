using System.Web.Mvc;
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
            public string Message { get; set; }
        }
        
        public ActionResult Index()
        {
            var status = underConstructionInfo;

            if (status.Status == UnderConstructionStatus.Finished)
            {
                return Redirect(Url.Content("~/"));
            }

            var model = new UnderConstructionModel()
            {
                Title = UnderConstruction.UnderConstructionTitle,
                Message = status.Message ?? UnderConstruction.ServerInitializing
            };

            return View(model);
        }
    }
}
