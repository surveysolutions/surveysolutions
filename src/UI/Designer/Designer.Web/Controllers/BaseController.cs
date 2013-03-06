using Main.Core.View;
using System.Web.Mvc;

namespace Designer.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public IViewRepository Repository { get; set; }

        protected BaseController(IViewRepository repository)
        {
            Repository = repository;
        }
    }
}
