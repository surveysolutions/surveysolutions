// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The question controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using RavenQuestionnaire.Web.Utils;

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Kaliko.ImageLibrary;
    using Kaliko.ImageLibrary.Filters;

    using Main.Core.Commands.File;
    using Main.Core.Commands.Questionnaire;
    using Main.Core.Commands.Questionnaire.Question;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.Answer;
    using Main.Core.View.Card;
    using Main.Core.View.Question;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Security;

    using RavenQuestionnaire.Core.Views.Event.File;
    using RavenQuestionnaire.Core.Views.Group;
    using RavenQuestionnaire.Web.Models;

    /// <summary>
    /// The question controller.
    /// </summary>
    [Authorize]
    [ValidateInput(false)]
    public class QuestionController : Controller
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
        /// Initializes a new instance of the <see cref="QuestionController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public QuestionController(IViewRepository viewRepository)
        {
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
            this.viewRepository = viewRepository;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add cards.
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
        public ActionResult AddCards(Guid publicKey, string questionnaireId)
        {
            return this.View(
                "_AddCards", new ImageNewViewModel { PublicKey = publicKey, QuestionnaireId = questionnaireId });
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Create(string id, Guid? groupPublicKey)
        {
            if (!groupPublicKey.HasValue || groupPublicKey == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            Guid questionnaireKey;
            if (!Guid.TryParse(id, out questionnaireKey))
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            GroupView group = this.viewRepository.Load<GroupViewInputModel, GroupView>(
                    new GroupViewInputModel(groupPublicKey.Value, questionnaireKey));
            this.LoadImages();
            var question = new QuestionView(id, groupPublicKey) { Parent = group.PublicKey, GroupTitle = group.Title };
            //return this.View("_Create", question);
            return this.View("Create", question);
        }

        /// <summary>
        /// Display partial view for question type
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="questionId">
        /// The question id.
        /// </param>
        /// <param name="QuestionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <returns>
        /// PartialView for question type
        /// </returns>
        public ActionResult ShowBlock(string type, Guid questionId, string QuestionnaireId, Guid? groupPublicKey)
        {
            var typeOfEnum = (QuestionType)Enum.Parse(typeof(QuestionType), type);
            var view = questionId == Guid.Empty ? new QuestionView(QuestionnaireId, groupPublicKey) : this.viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(questionId, Guid.Parse(QuestionnaireId)));
            switch (typeOfEnum)
            {
                case QuestionType.DropDownList: return this.PartialView("MultiSelectBlock", new MultyOptionsQuestionView(view));
                case QuestionType.MultyOption: return this.PartialView("MultiSelectBlock", new MultyOptionsQuestionView(view));
                case QuestionType.SingleOption: return this.PartialView("MultiSelectBlock", new MultyOptionsQuestionView(view));
                case QuestionType.YesNo: return this.PartialView("MultiSelectBlock", new MultyOptionsQuestionView(view));
                case QuestionType.AutoPropagate:
                    this.ViewBag.Group = view.Groups;
                    this.ViewBag.CurrentGroup = view.Parent;
                    return this.PartialView("_EditAutoPropagates", new AutoPropagateQuestionView(view));
                default:
                    return Content("<div id=\"additionalInfo\"></div>");
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Delete(Guid publicKey, Guid parentPublicKey, string questionnaireId)
        {
            this.commandService.Execute(new DeleteQuestionCommand(publicKey, parentPublicKey, Guid.Parse(questionnaireId)));
            return this.RedirectToAction("Details", "Questionnaire", new { id = questionnaireId });

        }

        /// <summary>
        /// The delete card.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        /// <returns>
        /// The delete card.
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [AcceptVerbs(HttpVerbs.Post)]
        public string DeleteCard(Guid publicKey, string questionnaireId, Guid imageKey)
        {
            this.commandService.Execute(new DeleteImageCommand(Guid.Parse(questionnaireId), publicKey, imageKey));
            return string.Empty;
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireKey">
        /// The questionnaire key.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        /// <exception cref="HttpException">
        /// </exception>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(Guid? publicKey, Guid? questionnaireKey)
        {
            if (!publicKey.HasValue || publicKey == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            if (!questionnaireKey.HasValue || questionnaireKey == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            this.LoadImages();
            QuestionView model = this.viewRepository.Load<QuestionViewInputModel, QuestionView>(new QuestionViewInputModel(publicKey.Value, questionnaireKey.Value));
            var transformator = new ExpressionReplacer(this.viewRepository);

            model.ConditionExpression = transformator.ReplaceGuidsWithStataCaptions(model.ConditionExpression, questionnaireKey.Value);
            model.ValidationExpression = transformator.ReplaceGuidsWithStataCaptions(model.ValidationExpression, questionnaireKey.Value);

            this.ViewBag.Group = model.Groups;
            this.ViewBag.CurrentGroup = model.Parent;
            //return this.PartialView("_Create", model);
            return this.PartialView("Create", model);
        }

        /// <summary>
        /// The edit card.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="imageKey">
        /// The image key.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult EditCard(Guid publicKey, string questionnaireId, Guid imageKey)
        {
            CardView source = this.viewRepository.Load<CardViewInputModel, CardView>(new CardViewInputModel(publicKey, questionnaireId, imageKey));
            return this.View(
                "_EditCard",
                new ImageNewViewModel
                    {
                        Desc = source.Description,
                        Title = source.Title,
                        QuestionnaireId = questionnaireId,
                        PublicKey = publicKey,
                        ImageKey = imageKey
                    });
        }

        /// <summary>
        /// The edit card.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCard(ImageNewViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                this.commandService.Execute(
                    new UpdateImageCommand(
                        Guid.Parse(model.QuestionnaireId), model.PublicKey, model.ImageKey, model.Title, model.Desc));
            }

            return View("_EditCard", model);
        }

        /// <summary>
        /// The move.
        /// </summary>
        /// <param name="PublicKeyQuestion">
        /// The public key question.
        /// </param>
        /// <param name="QuestionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpGet]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Move(Guid PublicKeyQuestion, Guid QuestionnaireId)
        {
            var model = new MoveItemModel { publicKey = PublicKeyQuestion, questionnaireId = QuestionnaireId };
            return View("MoveQuestion", model);
        }

        /// <summary>
        /// The move.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Move(MoveItemModel model)
        {
            this.commandService.Execute(
                new MoveQuestionnaireItemCommand(
                    model.questionnaireId, model.publicKey, model.groupGuid, model.afterGuid));
            return this.RedirectToAction(
                "Details", "Questionnaire", new { id = model.questionnaireId, qid = model.publicKey });
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionView[] question, AnswerView[] answers, IEnumerable<Guid> triggers)
        {
            QuestionView model = question[0];
            if (this.ModelState.IsValid)
            {
                try
                {
                    var ansverItems = new Answer[0];
                    if (answers != null)
                    {
                        ansverItems = answers.Select(ConvertAnswer).ToArray();
                    }

                    var transformator = new ExpressionReplacer(this.viewRepository);

                    var conditionExpression = transformator.ReplaceStataCaptionsWithGuids(model.ConditionExpression, model.QuestionnaireKey);
                    var validationExpression = transformator.ReplaceStataCaptionsWithGuids(model.ValidationExpression, model.QuestionnaireKey);

                    if (model.PublicKey == Guid.Empty)
                    {
                        Guid newItemKey = Guid.NewGuid();

                        // new fw
                        var commandService = NcqrsEnvironment.Get<ICommandService>();

                        if (triggers == null || !triggers.Any())
                        {
                            commandService.Execute(
                                new AddQuestionCommand(
                                    model.QuestionnaireKey,
                                    newItemKey,
                                    model.Title,
                                    model.StataExportCaption,
                                    model.QuestionType,
                                    model.QuestionScope,
                                    model.Parent,
                                    conditionExpression,
                                    validationExpression,
                                    model.ValidationMessage,
                                    model.Instructions,
                                    model.Featured,
                                    model.Mandatory,
                                    model.Capital,
                                    model.AnswerOrder,
                                    ansverItems,
                                    model.MaxValue));
                        }
                        else
                        {
                            commandService.Execute(
                                new AddQuestionCommand(
                                    model.QuestionnaireKey,
                                    newItemKey,
                                    model.Title,
                                    triggers.Where(t => t != Guid.Empty).Distinct().ToList(),
                                    model.MaxValue,
                                    model.StataExportCaption,
                                    model.QuestionType,
                                    model.QuestionScope,
                                    model.Parent,
                                    conditionExpression,
                                    validationExpression,
                                    model.ValidationMessage,
                                    model.Instructions,
                                    model.Featured,
                                    model.Mandatory,
                                    model.Capital,
                                    model.AnswerOrder,
                                    ansverItems));
                        }

                        model.PublicKey = newItemKey;
                    }
                    else
                    {
                        // new fw
                        var commandService = NcqrsEnvironment.Get<ICommandService>();
                        if (triggers == null || triggers.ToList().Count == 0)
                        {
                            commandService.Execute(
                                new ChangeQuestionCommand(
                                    model.QuestionnaireKey,
                                    model.PublicKey,
                                    model.Title,
                                    model.StataExportCaption,
                                    model.QuestionType,
                                    model.QuestionScope,
                                    conditionExpression,
                                    validationExpression,
                                    model.ValidationMessage,
                                    model.Instructions,
                                    model.Featured,
                                    model.Mandatory,
                                    model.Capital,
                                    model.AnswerOrder,
                                    ansverItems,
                                    model.MaxValue));
                        }
                        else
                        {
                            commandService.Execute(
                                new ChangeQuestionCommand(
                                    model.QuestionnaireKey,
                                    model.PublicKey,
                                    model.Title,
                                    triggers.Where(t => t != Guid.Empty).Distinct().ToList(),
                                    model.MaxValue,
                                    model.StataExportCaption,
                                    model.QuestionType,
                                    model.QuestionScope,
                                    conditionExpression,
                                    validationExpression,
                                    model.ValidationMessage,
                                    model.Instructions,
                                    model.Featured,
                                    model.Mandatory,
                                    model.Capital,
                                    model.AnswerOrder,
                                    ansverItems));
                        }
                    }
                }
                catch (DomainException e)
                {
                    this.AddModelErrorUsingDomainException(model, e);
                }
                catch (Exception e)
                {
                    if (e.InnerException is DomainException)
                    {
                        this.AddModelErrorUsingDomainException(model, (DomainException) e.InnerException);
                    }
                    else
                    {
                        this.AddModelErrorUsingBasicException(model, e);
                    }
                }

                if (this.ModelState.IsValid)
                {
                    return this.RedirectToAction("Details", "Questionnaire", new { id = model.QuestionnaireKey, qid = model.PublicKey });
                }
            }

            return View("Create", model);
        }

        private void AddModelErrorUsingBasicException(QuestionView model, Exception e)
        {
            this.ModelState.AddModelError(string.Format("question[{0}].ConditionExpression", model.PublicKey), e.Message);
        }

        private void AddModelErrorUsingDomainException(QuestionView model, DomainException e)
        {
            this.ModelState.AddModelError(string.Format("question[{0}].ConditionExpression", model.PublicKey), e.Message);
        }

        /// <summary>
        /// The upload card.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadCard(HttpPostedFileBase file, ImageNewViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                if (file != null)
                {
                    /*  var image = new KalikoImage(file.InputStream);
                    int thumbWidth, thumbHeight, origWidth, origHeight;
                    var thumbData = ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                    var origData = ResizeImage(image, 1024, 768, out origWidth, out origHeight);*/
                    Guid imageKey = Guid.NewGuid();
                    this.commandService.Execute(
                        new UploadFileCommand(imageKey, model.Title, model.Desc, file.InputStream));
                    this.commandService.Execute(
                        new UploadImageCommand(
                            model.PublicKey, Guid.Parse(model.QuestionnaireId), model.Title, model.Desc, imageKey));

                    return this.RedirectToAction("Details", "Questionnaire", new { id = model.QuestionnaireId });
                }

                this.ModelState.AddModelError("file", "Please select a file for upload");
            }

            return this.View("_AddCards");
        }

        /// <summary>
        /// The _ get answers.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="targetPublicKey">
        /// The target public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// </returns>
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        [HttpPost]
        public ActionResult _GetAnswers(Guid publicKey, Guid targetPublicKey, Guid questionnaireId)
        {
            QuestionView source =
                this.viewRepository.Load<QuestionViewInputModel, QuestionView>(
                    new QuestionViewInputModel(publicKey, questionnaireId));
            return this.PartialView(
                "_GetAnswers", new QuestionConditionModel { Source = source, TargetPublicKey = targetPublicKey });
        }


        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult CreatePropagateGroup(Guid questionPublicKey, Guid questionnaireId, Guid? groupPublicKey)
        {
            var input = questionPublicKey == Guid.Empty
                ? new QuestionViewInputModel(questionPublicKey, questionnaireId, groupPublicKey)
                : new QuestionViewInputModel(questionPublicKey, questionnaireId);
            var source =
                this.viewRepository.Load<QuestionViewInputModel, QuestionView>(input);
            this.ViewBag.Group = source.Groups;
            return this.PartialView("_AutoPropagateRow", Guid.NewGuid());
        }


        #endregion

        #region Methods

        /// <summary>
        /// The convert answer.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <returns>
        /// </returns>
        private static Answer ConvertAnswer(AnswerView a)
        {
            var answer = new Answer();
            answer.AnswerValue = a.AnswerValue;
            answer.AnswerType = a.AnswerType;
            answer.AnswerText = a.Title;
            answer.Mandatory = a.Mandatory;
            answer.PublicKey = a.PublicKey;
            answer.AnswerImage = a.AnswerImage;

            // on designer value is not set
            // answer.AnswerValue = a.AnswerValue;

            return answer;
        }

        /// <summary>
        /// The load images.
        /// </summary>
        private void LoadImages()
        {
            FileBrowseView images =
                this.viewRepository.Load<FileBrowseInputModel, FileBrowseView>(
                    new FileBrowseInputModel { PageSize = int.MaxValue });
            if (images != null)
            {
                var imagesList =
                    new SelectList(
                        images.Items.Select(
                            i => new SelectListItem { Selected = false, Text = i.FileName, Value = i.FileName }).ToList(),
                        "Value",
                        "Text");
                this.ViewBag.Images = imagesList;
            }
        }

        /// <summary>
        /// The resize image.
        /// </summary>
        /// <param name="image">
        /// The image.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="newWidth">
        /// The new width.
        /// </param>
        /// <param name="newHeight">
        /// The new height.
        /// </param>
        /// <returns>
        /// </returns>
        private Stream ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            KalikoImage thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));
            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;

            // var thumbData = new byte[ms.Length];
            // ms.Read(thumbData, 0, thumbData.Length);
            newHeight = thumb.Height;
            newWidth = thumb.Width;
            return ms;
        }

        #endregion
    }
}