#region

using System;
using System.Linq;
using System.Web.Mvc;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Answer;
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
            return PartialView("_EditRow", new AnswerView {QuestionId = questionPublicKey, PublicKey = Guid.NewGuid()});
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
    }
}