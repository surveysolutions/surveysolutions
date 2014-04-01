using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Interview.Views.TakeNew;
using WB.UI.Headquarters.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class InterviewsController : Controller
    {
        private readonly IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory;

        public InterviewsController(IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> takeNewInterviewViewFactory)
        {
            if (takeNewInterviewViewFactory == null) throw new ArgumentNullException("takeNewInterviewViewFactory");
            this.takeNewInterviewViewFactory = takeNewInterviewViewFactory;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TakeNew(Guid id)
        {
            Guid questionnaireId = id;
            Guid userId = User.UserId();
            TakeNewInterviewView model = this.takeNewInterviewViewFactory.Load(new TakeNewInterviewInputModel(questionnaireId, userId));
            
            return this.View(model);
        }
    }
}