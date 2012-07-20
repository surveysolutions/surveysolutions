#region

using System;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Questionnaire;
using System.Collections.Generic;

#endregion

namespace RavenQuestionnaire.Web.Controllers
{
    public class GroupController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IViewRepository viewRepository;

        public GroupController(IViewRepository viewRepository)
        {
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
            this.viewRepository = viewRepository;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id, Guid? parentGroup)
        {
            return View("_Create", new GroupView(id, parentGroup));
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
                        var newItemKey = Guid.NewGuid();
                        commandService.Execute(new AddGroupCommand(Guid.Parse(model.QuestionnaireId), newItemKey,
                            model.Title, model.Propagated, model.Parent, model.ConditionExpression));
                    }
                    else
                    {
                        commandService.Execute(new UpdateGroupCommand(model.Title, model.Propagated,
                                                                      model.QuestionnaireId,
                                                                      model.PublicKey, GlobalInfo.GetCurrentUser(), model.ConditionExpression));
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("ConditionExpression", e.Message);
                    return PartialView("_Create", model);
                }
                /* var questionnaire =
                     viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                         new QuestionnaireViewInputModel(model.QuestionnaireId));
                 */
                return RedirectToAction("Details", "Questionnaire", new { id = model.QuestionnaireId });
            }
            return View("_Create", model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid publicKey, string questionnaireId)
        {
            if (publicKey == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<GroupViewInputModel, GroupView>(new GroupViewInputModel(publicKey, questionnaireId));

            return View("_Create", model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public string Delete(Guid publicKey, string questionnaireId)
        {
            commandService.Execute(new DeleteGroupCommand(publicKey, Guid.Parse(questionnaireId)));
            return "";
        }
    }
}