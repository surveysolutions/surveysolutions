using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Resources;

namespace WB.UI.Shared.Web.Controllers
{
    [NoTransaction]
    public class UnderConstructionController : Controller
    {
        public class UnderConstructionModel
        {
            public string Title { get; set; }
            public string Message { get; set; }
        }

        [NoTransaction]
        public ActionResult Index()
        {
            var status = ServiceLocator.Current.GetInstance<UnderConstructionInfo>();

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
