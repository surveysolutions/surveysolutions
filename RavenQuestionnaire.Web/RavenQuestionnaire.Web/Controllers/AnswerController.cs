using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Web.Models;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class AnswerController : Controller
    {
         private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public AnswerController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(Guid questionPublicKey)
        {
            return PartialView("_EditRow", new AnswerView(){ QuestionId = questionPublicKey, PublicKey = Guid.NewGuid()});
        }
    }
}
