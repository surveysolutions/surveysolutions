using System;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public QuestionController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id)
        {
            return PartialView("_Create",
                               QuestionView.New(id));
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid publicKey, string questionnaireId)
        {
            if (publicKey== Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(publicKey, questionnaireId));

            return PartialView("_Create", model);
        }

        //
        // POST: /Questionnaire/Create
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionView model)
        {
            
            if (ModelState.IsValid)
            {
                try
                {


                    if (model.PublicKey == Guid.Empty)
                    {
                        AddNewQuestionCommand createCommand = new AddNewQuestionCommand(model.QuestionText,
                                                                                        model.QuestionType,
                                                                                        model.QuestionnaireId, null,
                                                                                        model.ConditionExpression,
                                                                                        model.Answers, Global.GetCurrentUser());
                        commandInvoker.Execute(createCommand);


                    }
                    else
                    {
                        commandInvoker.Execute(new UpdateQuestionCommand(model.QuestionnaireId, model.PublicKey,
                                                                         model.QuestionText, model.QuestionType,
                                                                         model.ConditionExpression, model.Answers,
                                                                         Global.GetCurrentUser()));
                    }
                }
                catch (Exception e)
                {

                    ModelState.AddModelError("ConditionExpression", e.Message);
                    return PartialView("_Create", model);
                }
                var questionnaire = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(model.QuestionnaireId));

                return PartialView("_Index", questionnaire.Questions);

            }
            return PartialView("_Create" , model);
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public string Delete(Guid publicKey, string questionnaireId)
        {
            commandInvoker.Execute(new DeleteQuestionCommand(publicKey, questionnaireId, Global.GetCurrentUser()));
            return "";
        }
    }
}
