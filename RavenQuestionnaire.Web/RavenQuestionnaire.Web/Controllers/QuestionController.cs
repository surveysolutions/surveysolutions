using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using Kaliko.ImageLibrary;
using RavenQuestionnaire.Core;
using Kaliko.ImageLibrary.Filters;
using RavenQuestionnaire.Web.Models;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Views.Card;
using RavenQuestionnaire.Core.Views.File;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    [ValidateInput(false)]
    public class QuestionController : Controller
    {

        #region Properties

        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        #endregion

        #region Constructor

        public QuestionController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        #endregion

        #region PublicMethod

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [AcceptVerbs(HttpVerbs.Post)]
        public string DeleteCard(Guid publicKey, string questionnaireId, Guid imageKey)
        {
            commandInvoker.Execute(new DeleteImageCommand(questionnaireId, publicKey, imageKey, GlobalInfo.GetCurrentUser()));
            return string.Empty;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult EditCard(Guid publicKey, string questionnaireId, Guid imageKey)
        {
            var source = viewRepository.Load<CardViewInputModel, CardView>(new CardViewInputModel(publicKey, questionnaireId, imageKey));
            return View("_EditCard", new ImageNewViewModel()
            {
                Desc = source.Description,
                Title = source.Title,
                QuestionnaireId = questionnaireId,
                PublicKey = publicKey,
                ImageKey = imageKey
            });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Move(MoveItemModel model)
        {
            commandInvoker.Execute(new MoveQuestionnaireItemCommand(model.questionnaireId, model.publicKey, model.groupGuid, model.afterGuid, GlobalInfo.GetCurrentUser()));
            return RedirectToAction("Details", "Questionnaire", new { id = model.questionnaireId });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCard(ImageNewViewModel model)
        {
            if (ModelState.IsValid)
                commandInvoker.Execute(new UpdateImageCommand(model.QuestionnaireId, model.PublicKey, model.ImageKey,
                                                              model.Title, model.Desc, GlobalInfo.GetCurrentUser()));
            return View("_EditCard", model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadCard(HttpPostedFileBase file, ImageNewViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var image = new KalikoImage(file.InputStream);
                    int thumbWidth, thumbHeight, origWidth, origHeight;
                    var thumbData = ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                    var origData = ResizeImage(image, 1024, 768, out origWidth, out origHeight);

                    commandInvoker.Execute(new UploadImageCommand(model.PublicKey, model.QuestionnaireId,
                                                                  model.Title, model.Desc,
                                                                  thumbData, thumbWidth, thumbHeight,
                                                                  origData, origWidth, origHeight,
                                                                  GlobalInfo.GetCurrentUser()));

                    return RedirectToAction("Details", "Questionnaire", new { id = model.QuestionnaireId });
                }
                ModelState.AddModelError("file", "Please select a file for upload");
            }
            return View("_AddCards");
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult AddCards(Guid publicKey, string questionnaireId)
        {
            return View("_AddCards", new ImageNewViewModel { PublicKey = publicKey, QuestionnaireId = questionnaireId });
        }


        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [HttpPost]
        public ActionResult _GetAnswers(Guid publicKey, Guid targetPublicKey, string questionnaireId)
        {
            var source = viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(publicKey, questionnaireId));
            return PartialView("_GetAnswers", new QuestionConditionModel
            {
                Source = source,
                TargetPublicKey = targetPublicKey
            });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id, Guid? groupPublicKey)
        {
            LoadImages();
            return View("_Create",
                               new QuestionView(id, groupPublicKey));
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid publicKey, string questionnaireId)
        {
            LoadImages();
            if (publicKey == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(publicKey, questionnaireId));
            return PartialView("_Create", model);
        }

        //
        // POST: /Questionnaire/Create
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionView[] question, AnswerView[] answers)
        {
            QuestionView model = question[0];
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.PublicKey == Guid.Empty)
                    {
                        AddNewQuestionCommand createCommand = new AddNewQuestionCommand(model.Title,
                                                                                        model.StataExportCaption,
                                                                                        model.QuestionType,
                                                                                        model.QuestionnaireId,
                                                                                        model.Parent,
                                                                                        model.ConditionExpression,
                                                                                        model.ValidationExpression,
                                                                                        model.Instructions,
                                                                                        model.Featured,
                                                                                        model.AnswerOrder,
                                                                                        answers,
                                                                                        GlobalInfo.GetCurrentUser());
                        commandInvoker.Execute(createCommand);
                    }
                    else
                        commandInvoker.Execute(new UpdateQuestionCommand(model.QuestionnaireId, model.PublicKey,
                                                                         model.Title,
                                                                         model.StataExportCaption,
                                                                         model.QuestionType,
                                                                         model.ConditionExpression,
                                                                         model.ValidationExpression,
                                                                         model.Featured, model.Instructions,
                                                                         answers, model.AnswerOrder,
                                                                         GlobalInfo.GetCurrentUser()));
                }
                catch (Exception e)
                {

                    ModelState.AddModelError(string.Format("question[{0}].ConditionExpression", model.PublicKey),
                                             e.Message);
                    return PartialView("_Create", model);
                }
                return RedirectToAction("Details", "Questionnaire", new { id = model.QuestionnaireId });
                //     var questionnaire = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(model.QuestionnaireId));
                if (model.Parent.HasValue)
                {
                    var updatedGroup =
                        viewRepository.Load<GroupViewInputModel, GroupView>(
                            new GroupViewInputModel(model.Parent.Value, model.QuestionnaireId));

                    return PartialView("_Index", updatedGroup.Questions);
                }
                var questionnaire = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(model.QuestionnaireId));
                return PartialView("_Index", questionnaire.Questions);
            }
            return View("_Create", model);
        }
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public string Delete(Guid publicKey, string questionnaireId)
        {
            commandInvoker.Execute(new DeleteQuestionCommand(publicKey, questionnaireId, GlobalInfo.GetCurrentUser()));
            return "";
        }

        #endregion

        #region PrivateMethod
        
        private byte[] ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            var thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));
            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;
            var thumbData = new byte[ms.Length];
            ms.Read(thumbData, 0, thumbData.Length);
            newHeight = thumb.Height;
            newWidth = thumb.Width;
            return thumbData;
        }

        
        private void LoadImages()
        {
            var images = viewRepository.Load<FileBrowseInputModel, FileBrowseView>(new FileBrowseInputModel { PageSize = int.MaxValue });
            if (images != null)
            {
                var imagesList = new SelectList(images.Items.Select(i => new SelectListItem
                                                                             {
                                                                                 Selected = false,
                                                                                 Text = i.Id,
                                                                                 Value = i.Id
                                                                             }).ToList(), "Value", "Text");
                ViewBag.Images = imagesList;
            }
        }

        #endregion
    }
}
