namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    public class TestUnhandledExceptionController : Controller
    {
        public ActionResult Index()
        {
            throw new InvalidOperationException("This exception is throw by special test controller and is used to test unhandled exceptions handling.");
        }
    }
}