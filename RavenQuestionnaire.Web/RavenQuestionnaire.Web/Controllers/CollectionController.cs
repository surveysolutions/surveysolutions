using System;
using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Views.Collection;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CollectionItem;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class CollectionController : Controller
    {

        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public CollectionController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        public ActionResult Index(CollectionBrowseInputModel input)
        {
            var model = viewRepository.Load<CollectionBrowseInputModel, CollectionBrowseView>(input);
            return View(model);
        }

        [HttpGet]
        public ActionResult AddNewCollection()
        {
            var model=new CollectionView();
            return View("Edit", model);
        }

        [HttpPost]
        public ActionResult Delete(string collectionId)
        {
            commandInvoker.Execute(new DeleteCollectionCommand(collectionId, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public ActionResult Edit(string collectionId)
        {
            var model =
                viewRepository.Load<CollectionViewInputModel, CollectionView>(new CollectionViewInputModel(collectionId));
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CollectionView collection)
        {
            if (string.IsNullOrEmpty(collection.CollectionId))
                commandInvoker.Execute(new CreateNewCollectionCommand(collection.Name, collection.Items.Select(item => new CollectionItem(item.PublicKey, item.Key, item.Value)).ToList()));
            else
                commandInvoker.Execute(new UpdateCollectionCommand(collection.CollectionId, collection.Name, collection.Items.Select(item=>new CollectionItem(item.PublicKey, item.Key, item.Value)).ToList()));
            return RedirectToAction("Index");
        }

        public ActionResult AddItem(string collectionid)
        {
            return PartialView("EditCollectionItem", new CollectionItemView() { CollectionId = collectionid, PublicKey = Guid.NewGuid()});
        }


        public ActionResult DeleteItem(string collectionId, Guid id)
        {
            commandInvoker.Execute(new DeleteCollectionItemCommand(collectionId, GlobalInfo.GetCurrentUser(), id));
            return RedirectToAction("Edit", new { collectionId = collectionId});
        }
    }
}
