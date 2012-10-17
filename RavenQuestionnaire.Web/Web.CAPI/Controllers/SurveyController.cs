// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyController.cs" company="">
//   
// </copyright>
// <summary>
//   The survey controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Core.CAPI.Views.PropagatedGroupViews.QuestionItemView;
using Main.Core.View.Answer;
using Main.Core.View.CompleteQuestionnaire.ScreenGroup;

namespace Web.CAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;

    using Core.CAPI.Views.Grouped;
    using Core.CAPI.Views.Json;

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Commands.Questionnaire.Group;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.CompleteQuestionnaire.Statistics;
    using Main.Core.View.Group;
    using Main.Core.View.Question;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using NLog;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Security;

    using Web.CAPI.Models;

    using LogManager = NLog.LogManager;

    /// <summary>
    /// The survey controller.
    /// </summary>
    [Authorize]
    public class SurveyController : Controller
    {
        #region Fields

        /// <summary>
        /// The _global provider.
        /// </summary>
        private readonly IGlobalInfoProvider _globalProvider;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="globalProvider">
        /// The global provider.
        /// </param>
        public SurveyController(IViewRepository viewRepository, IGlobalInfoProvider globalProvider)
        {
            this.viewRepository = viewRepository;
            this._globalProvider = globalProvider;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The answered.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.PartialViewResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public PartialViewResult Answered(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return this.PartialView("Complete/_Answered", stat);
        }

        /// <summary>
        /// The complete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public ActionResult Complete(string id, string comments)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            var key = new Guid();
            if (!Guid.TryParse(id, out key))
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id));

            var commandService = NcqrsEnvironment.Get<ICommandService>();
            SurveyStatus status;
            if (stat.InvalidQuestions.Count > 0)
            {
                status = SurveyStatus.Error;
            }
            else
            {
                status = SurveyStatus.Complete;
            }

            status.ChangeComment = comments;
            commandService.Execute(new ChangeStatusCommand { CompleteQuestionnaireId = key, Status = status });
            return this.RedirectToAction("Dashboard");
        }

        /// <summary>
        /// The complete summary.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.PartialViewResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public PartialViewResult CompleteSummary(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return this.PartialView("Complete/_Main", stat);
        }

        /// <summary>
        /// The dashboard.
        /// </summary>
        /// <returns>
        /// The System.Web.Mvc.ViewResult.
        /// </returns>
        public ViewResult Dashboard()
        {
            var user = this._globalProvider.GetCurrentUser();
            var inputModel = new CQGroupedBrowseInputModel();
            if (user != null)
            {
                inputModel.InterviewerId = user.Id;
            }

            CQGroupedBrowseView model =
                this.viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(inputModel);
            return View(model);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        public ActionResult Delete(string id)
        {
            var service = NcqrsEnvironment.Get<ICommandService>();
            service.Execute(new DeleteCompleteQuestionnaireCommand(Guid.Parse(id)));
            return this.RedirectToAction("Dashboard", "Survey");
        }

        /// <summary>
        /// The delete propagated group.
        /// </summary>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="parentGroupPublicKey">
        /// The parent group public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.JsonResult.
        /// </returns>
        public JsonResult DeletePropagatedGroup(
            Guid propagationKey, Guid publicKey, Guid parentGroupPublicKey, string questionnaireId)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(
                new DeletePropagatableGroupCommand(Guid.Parse(questionnaireId), propagationKey, publicKey));
            return this.Json(new { propagationKey });
        }
        protected ScreenGroupView GetGroup(Guid questionnaireId, Guid? groupid, Guid? propagationKey)
        {
            return
                this.viewRepository.Load<CompleteQuestionnaireViewInputModel, ScreenGroupView>(
                    new CompleteQuestionnaireViewInputModel(questionnaireId)
                        {
                            CurrentGroupPublicKey = groupid,
                            PropagationKey = propagationKey
                        });

        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ViewResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public ViewResult Index(Guid id, Guid? group, Guid? question, Guid? propagationKey)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            ScreenGroupView model = GetGroup(id, group, propagationKey);
            this.ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            this.ViewBag.PagePrefix = "page-to-delete";
            return View(model);
        }

        /// <summary>
        /// The invalid.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.PartialViewResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public PartialViewResult Invalid(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return this.PartialView("Complete/_Invalid", stat);
        }


        /// <summary>
        /// The participate.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
            {
                throw new HttpException("404");
            }

            Guid newQuestionnairePublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key));

            //asssign to executor
            commandService.Execute(
                new ChangeAssignmentCommand(newQuestionnairePublicKey, this._globalProvider.GetCurrentUser()));
            
            return this.RedirectToAction("Index", new { id = newQuestionnairePublicKey });
        }

        /// <summary>
        /// The propagate group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="parentGroupPublicKey">
        /// The parent group public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.JsonResult.
        /// </returns>
        public JsonResult PropagateGroup(Guid publicKey, Guid parentGroupPublicKey, Guid questionnaireId)
        {
            try
            {
                Guid propagationKey = Guid.NewGuid();
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new AddPropagatableGroupCommand(questionnaireId, propagationKey, publicKey));
                CompleteQuestionnaireJsonView model =
                    this.viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                        new CompleteQuestionnaireViewInputModel(questionnaireId)
                            {
                               CurrentGroupPublicKey = parentGroupPublicKey 
                            });
                return this.Json(new { propagationKey, parentGroupPublicKey = publicKey, group = model });
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("PropagationError", e.Message);
                return this.Json(new { error = e.Message, parentGroupPublicKey = publicKey });
            }
        }

        /// <summary>
        /// The re init.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        public ActionResult ReInit(string id)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(
                new ChangeStatusCommand { CompleteQuestionnaireId = Guid.Parse(id), Status = SurveyStatus.Initial });
            return this.RedirectToAction("Index", "Survey", new { id });
        }

        /// <summary>
        /// The save answer.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.JsonResult.
        /// </returns>
        public JsonResult SaveAnswer(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            CompleteQuestionView question = questions[0];

            List<Guid> answers = new List<Guid>();
            string completeAnswerValue = null;

            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();

                if (question.QuestionType == QuestionType.DropDownList ||
                question.QuestionType == QuestionType.SingleOption ||
                question.QuestionType == QuestionType.YesNo ||
                question.QuestionType == QuestionType.MultyOption)
                {
                    if (question.Answers != null && question.Answers.Length > 0)
                    {
                        for (int i = 0; i < question.Answers.Length; i++)
                        {
                            if (question.Answers[i].Selected)
                            {
                                answers.Add(question.Answers[i].PublicKey);
                            }
                        }
                    }
                }
                else
                {
                    completeAnswerValue = question.Answers[0].AnswerValue;
                }


                commandService.Execute(
                    new SetAnswerCommand(
                        settings[0].QuestionnaireId, 
                        question.PublicKey,
                        answers,
                        completeAnswerValue,
                        settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                this.logger.Fatal(e);
                var errorMessage = e.InnerException == null ? e.Message : e.InnerException.Message;
                return
                    this.Json(new { questionPublicKey = question.PublicKey, propogationPublicKey = settings[0].PropogationPublicKey, error = errorMessage });
            }

            var model = GetGroup(settings[0].QuestionnaireId, settings[0].ParentGroupPublicKey, settings[0].PropogationPublicKey);
            return this.Json(model);
        }
        /// <summary>
        /// The save answer.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.JsonResult.
        /// </returns>
        public JsonResult SaveGroupAnswer(Guid questionnaireId,
            Guid publicKey,
            Guid propogationPublicKey,
            Guid parentGroupPublicKey,
            QuestionType questionType,
            CompleteAnswerView[] answers)
        {
            //  CompleteQuestionView question = questions[0];

            List<Guid> answersGuid = new List<Guid>();
            string completeAnswerValue = null;

            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();

                if (questionType == QuestionType.DropDownList ||
                    questionType == QuestionType.SingleOption ||
                    questionType == QuestionType.YesNo ||
                    questionType == QuestionType.MultyOption)
                {
                    if (answers != null && answers.Length > 0)
                    {
                        for (int i = 0; i < answers.Length; i++)
                        {
                            if (answers[i].Selected)
                            {
                                answersGuid.Add(answers[i].PublicKey);
                            }
                        }
                    }
                }
                else
                {
                    completeAnswerValue = answers[0].AnswerValue;
                }


                commandService.Execute(
                    new SetAnswerCommand(
                        questionnaireId,
                        publicKey,
                        answersGuid,
                        completeAnswerValue,
                        propogationPublicKey));
            }
            catch (Exception e)
            {
                this.logger.Fatal(e);
                return
                    this.Json(
                        new
                            {
                                questionPublicKey = publicKey,
                                error = e.Message,
                                propogationPublicKey = propogationPublicKey
                            });
            }

            var model = GetGroup(questionnaireId, parentGroupPublicKey, null);
            return this.Json(model);
        }
        public JsonResult SaveGroupComment(Guid questionnaireId,
            Guid publicKey,
            Guid propogationPublicKey,
            Guid parentGroupPublicKey, string comment)
        {
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                Guid questionnaireKey = questionnaireId;
                commandService.Execute(
                    new SetCommentCommand(
                        questionnaireKey,
                        publicKey,
                        comment,
                        propogationPublicKey));
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return this.Json(new
                    {
                        questionPublicKey = publicKey,
                        error = e.Message,
                        propogationPublicKey = propogationPublicKey
                    });
            }

            var model = GetGroup(questionnaireId, parentGroupPublicKey, null);
            return this.Json(model);
        }
        /// <summary>
        /// The save comments.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.JsonResult.
        /// </returns>
        public JsonResult SaveComments(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            CompleteQuestionView question = questions[0];
            question.PublicKey = new Guid(this.Request.Form["PublicKey"]);
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                Guid questionnaireKey = settings[0].QuestionnaireId;
                commandService.Execute(
                    new SetCommentCommand(
                        questionnaireKey,
                        question.PublicKey,
                        question.Comments, 
                        settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return this.Json(new { questionPublicKey = questions[0].PublicKey, propogationPublicKey = settings[0].PropogationPublicKey, error = e.Message });
            }

            var model = GetGroup(settings[0].QuestionnaireId, settings[0].ParentGroupPublicKey, settings[0].PropogationPublicKey);
            return this.Json(model);
        }

        /// <summary>
        /// The screen.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.PartialViewResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey, Guid? question)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            ScreenGroupView model = GetGroup(id, group, propagationKey);
            this.ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            this.ViewBag.PagePrefix = string.Empty;
            return this.PartialView("_SurveyContent", model);
        }

        /// <summary>
        /// The statistic.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.ActionResult.
        /// </returns>
        public ActionResult Statistic(string id)
        {
            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id));
            return View(stat);
        }

        /// <summary>
        /// The unanswered.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The System.Web.Mvc.PartialViewResult.
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public PartialViewResult Unanswered(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid query string parameters");
            }

            CompleteQuestionnaireStatisticView stat =
                this.viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return this.PartialView("Complete/_Unanswered", stat);
        }

        #endregion
    }
}