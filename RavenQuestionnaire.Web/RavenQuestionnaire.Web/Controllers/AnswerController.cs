#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Collection;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.File;

#endregion


namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class AnswerController : Controller
    {
        private readonly IViewRepository viewRepository;
        private ICommandInvoker commandInvoker;

        public AnswerController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(Guid questionPublicKey)
        {
            LoadImages();
            return PartialView("_EditRow", new AnswerView { QuestionId = questionPublicKey, PublicKey = Guid.NewGuid() });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult CreateTabForDatabase(Guid questionPublicKey)
        {
            var res = viewRepository.Load<CollectionBrowseInputModel, CollectionBrowseView>(new CollectionBrowseInputModel());
            this.ViewBag.Collection = new SelectList(res.Items.ToList(), "Id", "Name");
            return PartialView("_EditDataBaseSettings", new AnswerView() { QuestionId = questionPublicKey, PublicKey = Guid.NewGuid() });
        }
        
        private void LoadImages()
        {
            var images =
                viewRepository.Load<FileBrowseInputModel, FileBrowseView>(new FileBrowseInputModel
                                                                              {PageSize = int.MaxValue});
            var imagesList = new SelectList(images.Items.Select(i => new SelectListItem
                                                                         {
                                                                             Selected = false,
                                                                             Text = i.Id,
                                                                             Value = i.Id
                                                                         }).ToList(), "Value", "Text");
            ViewBag.Images = imagesList;
        }


        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult FillAnswers(Guid questionPublicKey, string collectionId)
        {
            var list = new Dictionary<string,string>();
            //List<ListItem> list = new List<ListItem>()
            //                          {
            //                              new ListItem() {Value = "1", Text = "VA"},
            //                              new ListItem() {Value = "2", Text = "MD"},
            //                              new ListItem() {Value = "3", Text = "DC"}
            //                          };
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}