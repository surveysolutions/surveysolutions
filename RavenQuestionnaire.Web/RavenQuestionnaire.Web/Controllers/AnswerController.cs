// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnswerController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The answer controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.Answer;

    using Questionnaire.Core.Web.Security;

    using RavenQuestionnaire.Core.Views.Collection;
    using RavenQuestionnaire.Core.Views.CollectionItem;
    using RavenQuestionnaire.Core.Views.Event.File;

    /// <summary>
    /// The answer controller.
    /// </summary>
    [Authorize]
    public class AnswerController : Controller
    {
        #region Constants and Fields

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public AnswerController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(Guid questionPublicKey)
        {
            this.LoadImages();
            return this.PartialView(
                "_EditRow", new AnswerView { Parent = questionPublicKey, PublicKey = Guid.NewGuid() });
        }

        /// <summary>
        /// The create tab for database.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult CreateTabForDatabase(Guid questionPublicKey)
        {
            CollectionBrowseView res =
                this.viewRepository.Load<CollectionBrowseInputModel, CollectionBrowseView>(
                    new CollectionBrowseInputModel());
            this.ViewBag.Collection = new SelectList(res.Items.ToList(), "Id", "Name");
            return this.PartialView(
                "_EditDataBaseSettings", new AnswerView { Parent = questionPublicKey, PublicKey = Guid.NewGuid() });
        }

        /// <summary>
        /// The fill answers.
        /// </summary>
        /// <param name="NameCollection">
        /// The name collection.
        /// </param>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult FillAnswers(string NameCollection, Guid questionId)
        {
            CollectionItemBrowseView result =
                this.viewRepository.Load<CollectionItemBrowseInputModel, CollectionItemBrowseView>(
                    new CollectionItemBrowseInputModel(NameCollection, questionId));
            List<AnswerView> answers =
                result.Items.Select(
                    item =>
                    new AnswerView(
                        new Guid(), 
                        new Answer
                            {
                                AnswerText = item.Value, 
                                AnswerType = AnswerType.Select, 
                                AnswerValue = item.Key, 
                                Mandatory = false, 
                                NameCollection = NameCollection
                            })).ToList();
            return this.PartialView("_EditCollectionItem", answers);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The load images.
        /// </summary>
        private void LoadImages()
        {
            FileBrowseView images =
                this.viewRepository.Load<FileBrowseInputModel, FileBrowseView>(
                    new FileBrowseInputModel { PageSize = int.MaxValue });
            var imagesList =
                new SelectList(
                    images.Items.Select(
                        i => new SelectListItem { Selected = false, Text = i.FileName, Value = i.FileName }).ToList(), 
                    "Value", 
                    "Text");
            this.ViewBag.Images = imagesList;
        }

        #endregion
    }
}