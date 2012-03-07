#region

using System;
using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Questionnaire;

#endregion

namespace RavenQuestionnaire.Web.Controllers
{
    public class GroupController : Controller
    {
        private readonly ICommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public GroupController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id, Guid? parentGroup)
        {
            return PartialView("_Create",
                               new GroupView(id, parentGroup));
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
                        var createCommand = new CreateNewGroupCommand(model.GroupText, model.Propagated,
                                                                      model.QuestionnaireId, model.ParentGroup,
                                                                      GlobalInfo.GetCurrentUser());
                        commandInvoker.Execute(createCommand);
                    }
                    else
                    {
                        commandInvoker.Execute(new UpdateGroupCommand(model.GroupText, model.Propagated,
                                                                      model.QuestionnaireId,
                                                                      model.PublicKey, GlobalInfo.GetCurrentUser()));
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("ConditionExpression", e.Message);
                    return PartialView("_Create", model);
                }
                var questionnaire =
                    viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                        new QuestionnaireViewInputModel(model.QuestionnaireId));

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

        public ActionResult PropagateGroup(Guid publicKey, Guid parentGroupPublicKey, string questionnaireId)
        {
            try
            {
                commandInvoker.Execute(new PropagateGroupCommand(questionnaireId, publicKey, GlobalInfo.GetCurrentUser()));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("PropagationError", e.Message);
            }
            var model =
                viewRepository.Load<CompleteGroupViewInputModel, CompleteGroupView>(
                    new CompleteGroupViewInputModel(null, parentGroupPublicKey, questionnaireId));
            ViewBag.CurrentGroup = model;
            return PartialView("~/Views/Group/_Screen.cshtml", model);
            //   return RedirectToAction("Question", "CompleteQuestionnaire", new {id = questionnaireId});
        }

        public ActionResult DeletePropagatedGroup(Guid propagationKey, Guid publicKey, Guid parentGroupPublicKey,
                                                  string questionnaireId)
        {
            commandInvoker.Execute(new DeletePropagatedGroupCommand(questionnaireId, publicKey, propagationKey,
                                                                    GlobalInfo.GetCurrentUser()));

            var model =
                viewRepository.Load<CompleteGroupViewInputModel, CompleteGroupView>(
                    new CompleteGroupViewInputModel(null, parentGroupPublicKey, questionnaireId));
            ViewBag.CurrentGroup = model;
            return PartialView("~/Views/Group/_Screen.cshtml", model);
        }

        public ActionResult PropagateGroupV(Guid publicKey, Guid parentGroupPublicKey, string questionnaireId)
        {
            try
            {
                commandInvoker.Execute(new PropagateGroupCommand(questionnaireId, publicKey, GlobalInfo.GetCurrentUser()));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("PropagationError", e.Message);
            }

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>( new CompleteQuestionnaireViewInputModel(questionnaireId) { CurrentGroupPublicKey = parentGroupPublicKey });

            return PartialView("~/Views/Group/_ScreenV.cshtml", model);
        }

        public ActionResult DeletePropagatedGroupV(Guid propagationKey, Guid publicKey, Guid parentGroupPublicKey,
                                                  string questionnaireId)
        {
            commandInvoker.Execute(new DeletePropagatedGroupCommand(questionnaireId, publicKey, propagationKey,
                                                                    GlobalInfo.GetCurrentUser()));

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(new CompleteQuestionnaireViewInputModel(questionnaireId) { CurrentGroupPublicKey = parentGroupPublicKey });
            
            return PartialView("~/Views/Group/_ScreenV.cshtml", model);
        }

        public ActionResult PropagateGroupC(Guid publicKey, Guid parentGroupPublicKey, string questionnaireId)
        {
            try
            {
                commandInvoker.Execute(new PropagateGroupCommand(questionnaireId, publicKey, GlobalInfo.GetCurrentUser()));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("PropagationError", e.Message);
            }

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(new CompleteQuestionnaireViewInputModel(questionnaireId) { CurrentGroupPublicKey = parentGroupPublicKey });

            return PartialView("~/Views/Group/_ScreenC.cshtml", model);
        }

        public ActionResult DeletePropagatedGroupC(Guid propagationKey, Guid publicKey, Guid parentGroupPublicKey,
                                                  string questionnaireId)
        {
            commandInvoker.Execute(new DeletePropagatedGroupCommand(questionnaireId, publicKey, propagationKey,
                                                                    GlobalInfo.GetCurrentUser()));

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(new CompleteQuestionnaireViewInputModel(questionnaireId) { CurrentGroupPublicKey = parentGroupPublicKey });

            return PartialView("~/Views/Group/_ScreenC.cshtml", model);
        }
    
    }
}