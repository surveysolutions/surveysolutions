// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The collection controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Main.Core.Commands.Collection;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Security;

    using RavenQuestionnaire.Core.Views.Collection;
    using RavenQuestionnaire.Core.Views.CollectionItem;

    /// <summary>
    /// The collection controller.
    /// </summary>
    [Authorize]
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class CollectionController : Controller
    {
        #region Constants and Fields

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public CollectionController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add item.
        /// </summary>
        /// <param name="collectionid">
        /// The collectionid.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult AddItem(string collectionid)
        {
            return this.PartialView(
                "EditCollectionItem", new CollectionItemView { CollectionId = collectionid, PublicKey = Guid.NewGuid() });
        }

        /// <summary>
        /// The add new collection.
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet]
        public ActionResult AddNewCollection()
        {
            var model = new CollectionView();
            return View("Edit", model);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        public ActionResult Delete(string collectionId)
        {
            // commandInvoker.Execute(new DeleteCollectionCommand(collectionId, GlobalInfo.GetCurrentUser()));
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The delete item.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult DeleteItem(string collectionId, Guid id)
        {
            // commandInvoker.Execute(new DeleteCollectionItemCommand(collectionId, GlobalInfo.GetCurrentUser(), id));
            return this.RedirectToAction("Edit", new { collectionId });
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpGet]
        public ActionResult Edit(string collectionId)
        {
            CollectionView model =
                this.viewRepository.Load<CollectionViewInputModel, CollectionView>(
                    new CollectionViewInputModel(collectionId));
            return View(model);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        public ActionResult Edit(CollectionView collection)
        {
            if (string.IsNullOrEmpty(collection.CollectionId))
            {
                List<CollectionItem> items =
                    collection.Items.Select(item => new CollectionItem(item.PublicKey, item.Key, item.Value)).ToList();

                // commandInvoker.Execute(new CreateNewCollectionCommand(collection.Name, items ));
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new CreateCollectionCommand(Guid.NewGuid(), collection.Name, items));
            }
            else
            {
                // commandInvoker.Execute(new UpdateCollectionCommand(collection.CollectionId, collection.Name, collection.Items.Select(item=>new CollectionItem(item.PublicKey, item.Key, item.Value)).ToList()));
            }

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult Index(CollectionBrowseInputModel input)
        {
            CollectionBrowseView model =
                this.viewRepository.Load<CollectionBrowseInputModel, CollectionBrowseView>(input);
            return View(model);
        }

        #endregion
    }
}