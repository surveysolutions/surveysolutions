using System;
using System.Web.Mvc;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Interview.Views.TakeNew;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class InterviewsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult TakeNew(Guid id)
        {
            Guid key = id;

            var model = new TakeNewInterviewView(new QuestionnaireDocument(), 1);

            return this.View(model);
        }
    }
}