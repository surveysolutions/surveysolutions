using System.Web.Mvc;
using System.Web.UI.WebControls;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class QuestionnairesController : Controller
    {
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;

        public QuestionnairesController(IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        public ActionResult Index()
        {
            var model = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel {
                PageSize = 1024
            });
            return this.View(model);
        }
    }
}