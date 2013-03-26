using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using System.Web.Mvc;

namespace WB.UI.Designer.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IViewRepository Repository;
        protected readonly ICommandService CommandService;

        protected BaseController(IViewRepository repository, ICommandService commandService)
        {
            Repository = repository;
            CommandService = commandService;
        }
    }
}
