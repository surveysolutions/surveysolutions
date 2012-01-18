using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Web.Controllers
{
    public class GroupController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public GroupController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id)
        {
            return PartialView("_Create",
                               new GroupView(id));
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(GroupView model)
        {

            if (ModelState.IsValid)
            {
                try
                {


                    if (model.PublicKey == Guid.Empty)
                    {
                        CreateNewGroupCommand createCommand = new CreateNewGroupCommand(model.GroupText,
                                                                                        model.QuestionnaireId, null, GlobalInfo.GetCurrentUser());
                        commandInvoker.Execute(createCommand);


                    }
                    else
                    {
                        commandInvoker.Execute(new UpdateGroupCommand(model.GroupText, model.QuestionnaireId,
                                                                      model.PublicKey, GlobalInfo.GetCurrentUser()));
                    }
                }
                catch (Exception e)
                {

                    ModelState.AddModelError("ConditionExpression", e.Message);
                    return PartialView("_Create", model);
                }
                var questionnaire = 
                    viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(model.QuestionnaireId));

                return PartialView("_Index", questionnaire.Groups);

            }
            return PartialView("_Create", model);
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid publicKey, string questionnaireId)
        {
            if (publicKey == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<GroupViewInputModel, GroupView>(new GroupViewInputModel(publicKey, questionnaireId));

            return PartialView("_Create", model);
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public string Delete(Guid publicKey, string questionnaireId)
        {
            commandInvoker.Execute(new DeleteGroupCommand(publicKey, questionnaireId, GlobalInfo.GetCurrentUser()));
            return "";
        }
        public ActionResult PropagateGroup(Guid publicKey, string questionnaireId)
        {
            commandInvoker.Execute(new PropagateGroupCommand(questionnaireId, publicKey, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Question", "CompleteQuestionnaire", new {id = questionnaireId});
        }
        public ActionResult DeletePropagatedGroup(Guid propagationKey, Guid publicKey, string questionnaireId)
        {
            commandInvoker.Execute(new DeletePropagatedGroupCommand(questionnaireId, publicKey, propagationKey,
                                                                    GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Question", "CompleteQuestionnaire", new {id = questionnaireId});
        }
    }
}
