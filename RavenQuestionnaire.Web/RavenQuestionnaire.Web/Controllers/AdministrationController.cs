using System.Web.Mvc;
using Questionnaire.Core.Web.Security;
using Main.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Web.Controllers
{
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class AdministrationController : Controller
    {
        //
        // GET: /Administration/

        public ActionResult Menu()
        {
            return View();
        }

    }
}
