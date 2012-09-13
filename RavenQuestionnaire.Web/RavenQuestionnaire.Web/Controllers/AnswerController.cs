#region

using System;
using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Collection;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CollectionItem;

#endregion


namespace RavenQuestionnaire.Web.Controllers
{
    using RavenQuestionnaire.Core.Views.Event.File;

    [Authorize]
    public class AnswerController : Controller
    {
        private readonly IViewRepository viewRepository;

        public AnswerController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(Guid questionPublicKey)
        {
            LoadImages();
            return PartialView("_EditRow", new AnswerView { Parent = questionPublicKey, PublicKey = Guid.NewGuid() });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult CreateTabForDatabase(Guid questionPublicKey)
        {
            var res = viewRepository.Load<CollectionBrowseInputModel, CollectionBrowseView>(new CollectionBrowseInputModel());
            this.ViewBag.Collection = new SelectList(res.Items.ToList(), "Id", "Name");
            return PartialView("_EditDataBaseSettings", new AnswerView() { Parent = questionPublicKey, PublicKey = Guid.NewGuid() });
        }
        
        private void LoadImages()
        {
            var images =
                viewRepository.Load<FileBrowseInputModel, FileBrowseView>(new FileBrowseInputModel
                                                                              {PageSize = int.MaxValue});
            var imagesList = new SelectList(images.Items.Select(i => new SelectListItem
                                                                         {
                                                                             Selected = false,
                                                                             Text = i.FileName,
                                                                             Value = i.FileName
                                                                         }).ToList(), "Value", "Text");
            ViewBag.Images = imagesList;
        }


        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult FillAnswers(string NameCollection, Guid questionId)
        {
            var result = viewRepository.Load<CollectionItemBrowseInputModel, CollectionItemBrowseView>(new CollectionItemBrowseInputModel(NameCollection, questionId));
            var answers = result.Items.Select(item => new AnswerView(new Guid(), new Answer()
                                                                                     {
                                                                                         AnswerText = item.Value, 
                                                                                         AnswerType = AnswerType.Select, 
                                                                                         AnswerValue = item.Key,
                                                                                         Mandatory = false, 
                                                                                         NameCollection = NameCollection
                                                                                     })).ToList();
            return PartialView("_EditCollectionItem", answers);
        }
    }
}