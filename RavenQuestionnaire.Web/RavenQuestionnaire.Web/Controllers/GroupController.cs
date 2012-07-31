using Ncqrs;
using System;
using System.Web;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Web.Models;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;


namespace RavenQuestionnaire.Web.Controllers
{
    public class GroupController : Controller
    {
        #region Properties

        private readonly ICommandService commandService;
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructor

        public GroupController(IViewRepository viewRepository)
        {
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
            this.viewRepository = viewRepository;
        }

        #endregion

        #region Actions

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
                                                                      Guid.Parse(model.QuestionnaireId),
                                                                      model.PublicKey, GlobalInfo.GetCurrentUser(), model.ConditionExpression));
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("ConditionExpression", e.Message);
                    return PartialView("_Create", model);
                }
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

        [HttpGet]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Move(Guid PublicKeyGroup, Guid QuestionnaireId)
        {
            MoveItemModel model = new MoveItemModel() { publicKey = PublicKeyGroup, questionnaireId = QuestionnaireId };
            return View("MoveQuestion", model);
        }

        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Move(MoveItemModel model)
        {
            return null;
        }

        #endregion
    }
}