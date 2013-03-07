using Main.Core.View;
using System.Web.Mvc;

namespace WB.UI.Designer.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IViewRepository _repository;

        protected BaseController(IViewRepository repository)
        {
            _repository = repository;
        }
    }
}
