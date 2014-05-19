using System.Web.Mvc;
using Machine.Specifications;
using WB.UI.Supervisor.Controllers;

namespace WB.UI.Supervisor.Tests.HQSyncControllerTests
{
    [Subject(typeof(HQSyncController))]
    internal class HQSyncControllerTestsContext
    {
        protected static dynamic GetDataFromJsonResult(ActionResult result)
        {
            return ((JsonResult)result).Data;
        }
    }
}