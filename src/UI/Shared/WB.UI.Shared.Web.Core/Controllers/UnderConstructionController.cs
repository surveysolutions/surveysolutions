using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Controllers
{
    public class UnderConstructionController : Controller
    {
        private readonly UnderConstructionInfo underConstructionInfo;

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
                Title = Resources.UnderConstruction.UnderConstructionTitle,
                MainMessage = isError ? status.Message : Resources.UnderConstruction.ServerInitializing,
                SubMessage = isError ? null : status.Message
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Status()
        {
            await underConstructionInfo.WaitForFinish;
            return Ok();
        }
    }
}
