using System;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Models;

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
        [HttpPost]
        public ActionResult _GetAnswers(Guid publicKey, Guid targetPublicKey, string questionnaireId)
        {
            var source = viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(publicKey, questionnaireId));
            return PartialView("_GetAnswers", new QuestionConditionModel
                                                  {
                                                      Source = source,
                                                      TargetPublicKey = targetPublicKey
                                                  });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id, Guid? groupPublicKey)
        {
            return PartialView("_Create",
                               new QuestionView(id, groupPublicKey));
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid publicKey, string questionnaireId)
        {
            if (publicKey == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(publicKey, questionnaireId));

            return PartialView("_Create", model);
        }

        //
        // POST: /Questionnaire/Create
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionView[] question, AnswerView[] answers)
        {
            QuestionView model = question[0];
            if (ModelState.IsValid)
            {
                try
                {

                    if (model.PublicKey == Guid.Empty)
                    {
                        AddNewQuestionCommand createCommand = new AddNewQuestionCommand(model.QuestionText,
                                                                                        model.StataExportCaption,
                                                                                        model.QuestionType,
                                                                                        model.QuestionnaireId, model.GroupPublicKey,
                                                                                        model.ConditionExpression,
                                                                                        model.Instructions,
                                                                                        answers, GlobalInfo.GetCurrentUser());
                        commandInvoker.Execute(createCommand);


                    }
                    else
                    {
                        commandInvoker.Execute(new UpdateQuestionCommand(model.QuestionnaireId, model.PublicKey,
                                                                         model.QuestionText,
                                                                         model.StataExportCaption,
                                                                         model.QuestionType,
                                                                         model.ConditionExpression,
                                                                         model.Instructions,
                                                                         answers,
                                                                         GlobalInfo.GetCurrentUser()));
                    }
                }
                catch (Exception e)
                {

                    ModelState.AddModelError(string.Format("question[{0}].ConditionExpression", model.PublicKey),
                                             e.Message);
                    return PartialView("_Create", model);
                }
           //     var questionnaire = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(model.QuestionnaireId));
                if (model.GroupPublicKey.HasValue)
                {
                    var updatedGroup =
                        viewRepository.Load<GroupViewInputModel, GroupView>(
                            new GroupViewInputModel(model.GroupPublicKey.Value, model.QuestionnaireId));

                    return PartialView("_Index", updatedGroup.Questions);
                }
                else
                {
                    var questionnaire = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(model.QuestionnaireId));
                    return PartialView("_Index", questionnaire.Questions);
                }

            }
            return PartialView("_Create", model);
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public string Delete(Guid publicKey, string questionnaireId)
        {
            commandInvoker.Execute(new DeleteQuestionCommand(publicKey, questionnaireId, GlobalInfo.GetCurrentUser()));
            return "";
        }
    }
}
