// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The group controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire.Group;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Security;

    using RavenQuestionnaire.Core.Views.Group;
    using RavenQuestionnaire.Web.Models;

    /// <summary>
    /// The group controller.
    /// </summary>
    public class GroupController : Controller
    {
        #region Constants and Fields

        /// <summary>
        /// The command service.
        /// </summary>
        private readonly ICommandService commandService;

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public GroupController(IViewRepository viewRepository)
        {
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
            this.viewRepository = viewRepository;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="parentGroup">
        /// The parent group.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id, Guid? parentGroup)
        {
            return this.View("_Create", new GroupView(Guid.Parse(id), parentGroup));
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Delete(Guid publicKey, string questionnaireId)
        {
            this.commandService.Execute(new DeleteGroupCommand(publicKey, Guid.Parse(questionnaireId)));
            return this.RedirectToAction("Details", "Questionnaire", new { id = questionnaireId });
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid publicKey, Guid questionnaireId)
        {
            if (publicKey == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            GroupView model =
                this.viewRepository.Load<GroupViewInputModel, GroupView>(
                    new GroupViewInputModel(publicKey, questionnaireId));

            return View("_Create", model);
        }

        /// <summary>
        /// The move.
        /// </summary>
        /// <param name="PublicKeyGroup">
        /// The public key group.
        /// </param>
        /// <param name="QuestionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpGet]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Move(Guid PublicKeyGroup, Guid QuestionnaireId)
        {
            var model = new MoveItemModel { publicKey = PublicKeyGroup, questionnaireId = QuestionnaireId };
            return View("MoveQuestion", model);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(GroupView model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    if (model.PublicKey == Guid.Empty)
                    {
                        Guid newItemKey = Guid.NewGuid();
                        this.commandService.Execute(
                            new AddGroupCommand(
                                model.QuestionnaireKey, newItemKey, model.Title, model.Parent, model.ConditionExpression));
                    }
                    else
                    {
                        this.commandService.Execute(
                            new UpdateGroupCommand(
                                model.Title, 
                                model.Propagated, 
                                model.QuestionnaireKey, 
                                model.PublicKey, 
                                GlobalInfo.GetCurrentUser(), 
                                model.ConditionExpression));
                    }
                }
                catch (Exception e)
                {
                    this.ModelState.AddModelError("ConditionExpression", e.Message);
                    return this.PartialView("_Create", model);
                }

                return this.RedirectToAction("Details", "Questionnaire", new { id = model.QuestionnaireKey });
            }

            return View("_Create", model);
        }

        #endregion
    }
}