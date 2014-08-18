﻿using System.Collections.Generic;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize]
    public class QuestionnaireController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;


        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireViewInputModel, EditQuestionnaireView> editQuestionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;

        private readonly ILogger logger;

        public QuestionnaireController(
            ICommandService commandService,
            IMembershipUserService userHelper,
            IQuestionnaireVerifier questionnaireVerifier,
            IQuestionnaireHelper questionnaireHelper,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            ILogger logger, IViewFactory<QuestionnaireViewInputModel, EditQuestionnaireView> editQuestionnaireViewFactory,
            IExpressionProcessorGenerator expressionProcessorGenerator)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.questionnaireVerifier = questionnaireVerifier;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.logger = logger;
            this.editQuestionnaireViewFactory = editQuestionnaireViewFactory;
            //inject it
            this.expressionProcessorGenerator = expressionProcessorGenerator;
        }

        public ActionResult Clone(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            return
                this.View(
                    new QuestionnaireCloneModel { Title = string.Format("Copy of {0}", model.Title), Id = model.PublicKey });
        }

        [HttpPost]
        public JsonResult Verify(Guid id)
        {
            var questionnaireDocument = this.GetQuestionnaire(id).Source;
            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireDocument).ToArray();
            
            if (!questoinnaireErrors.Any())
            {
                GenerationResult generationResult;
                try
                {
                    string resultAssembly;
                    generationResult = this.expressionProcessorGenerator.GenerateProcessor(questionnaireDocument, out resultAssembly);
                }
                catch (Exception)
                {
                    generationResult = new GenerationResult()
                    {
                        Success = false,
                        Diagnostics = new List<GenerationDiagnostic>() { new GenerationDiagnostic("Common verifier error", "Error", GenerationDiagnosticSeverity.Error) }
                    };
                }
                //errors shouldn't be displayed as is 
                questoinnaireErrors = generationResult.Success
                    ? new QuestionnaireVerificationError[0]
                    : generationResult.Diagnostics.Select(d => new QuestionnaireVerificationError("WB1001", d.Message, new QuestionnaireVerificationReference[0])).ToArray();
            }

            return this.Json(new VerificationResult
            {
                Errors = questoinnaireErrors
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clone(QuestionnaireCloneModel model)
        {
            if (this.ModelState.IsValid)
            {
                QuestionnaireView sourceModel = this.GetQuestionnaire(model.Id);
                if (sourceModel == null)
                {
                    throw new ArgumentNullException("model");
                }
                try
                {
                    var questionnaireId = Guid.NewGuid();
                    this.commandService.Execute(
                        new CloneQuestionnaireCommand(questionnaireId, model.Title, UserHelper.WebUser.UserId,
                            model.IsPublic, sourceModel.Source));

                    return this.RedirectToAction("Open", "App", new { id = questionnaireId });
                }
                catch (Exception e)
                {
                    logger.Error("Error on questionnaire cloning.", e);

                    var domainException = e.GetSelfOrInnerAs<QuestionnaireException>();
                    if (domainException != null)
                    {
                        this.Error(domainException.Message);
                        logger.Error("Questionnaire controller -> clone: " + domainException.Message, domainException);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return this.View(model);
        }

        public ActionResult Create()
        {
            return this.View(new QuestionnaireViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionnaireViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var questionnaireId = Guid.NewGuid();
                this.commandService.Execute(
                    new CreateQuestionnaireCommand(
                        questionnaireId: questionnaireId,
                        text: model.Title,
                        createdBy: UserHelper.WebUser.UserId,
                        isPublic: model.IsPublic));
                return this.RedirectToAction("Open", "App", new { id = questionnaireId });
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            if ((model.CreatedBy != UserHelper.WebUser.UserId) && !UserHelper.WebUser.IsAdmin)
            {
                this.Error("You don't  have permissions to delete this questionnaire.");
            }
            else
            {
                this.commandService.Execute(new DeleteQuestionnaireCommand(model.PublicKey));
                this.Success(string.Format("Questionnaire \"{0}\" successfully deleted", model.Title));
            }

            return this.Redirect(this.Request.UrlReferrer.ToString());
        }

        public ActionResult Edit(Guid id)
        {
            EditQuestionnaireView questionnaire = this.editQuestionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if(questionnaire == null)
                throw new HttpException(404, string.Empty);

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = id });

            var isUserIsOwnerOrAdmin = questionnaire.CreatedBy == UserHelper.WebUser.UserId || UserHelper.WebUser.IsAdmin;
            var isQuestionnaireIsSharedWithThisPerson = (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == this.UserHelper.WebUser.UserId);

            if (isUserIsOwnerOrAdmin || isQuestionnaireIsSharedWithThisPerson)
            {
                this.ReplaceGuidsInValidationAndConditionRules(questionnaire);
            }
            else
            {
                throw new HttpException(403, string.Empty);
            }

            return
                View(new QuestionnaireEditView(questionaire: questionnaire, questionnaireSharedPersons: questionnaireSharedPersons, isOwner: questionnaire.CreatedBy == UserHelper.WebUser.UserId));
        }

        public ActionResult Index(int? p, string sb, int? so, string f)
        {
            return this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, filter: f));
        }

        public ActionResult Public(int? p, string sb, int? so, string f)
        {
            return this.View(this.GetPublicQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, filter: f));
        }

        private IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            int? pageIndex, string sortBy, int? sortOrder, string filter)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, filter: filter);

            return this.questionnaireHelper.GetPublicQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                filter: filter,
                viewerId: UserHelper.WebUser.UserId);
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire =
                this.questionnaireViewFactory.Load(
                    new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new HttpException(
                    (int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} cannot be found", id));
            }

            return questionnaire;
        }

        private IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            int? pageIndex, string sortBy, int? sortOrder, string filter)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, filter: filter);

            return this.questionnaireHelper.GetQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                filter: filter,
                viewerId: UserHelper.WebUser.UserId);
        }

        private void ReplaceGuidsInValidationAndConditionRules(EditQuestionnaireView model)
        {
            var expressionReplacer = new ExpressionReplacer(model);
            foreach (EditQuestionView question in model.Questions)
            {
                question.ConditionExpression =
                       expressionReplacer.ReplaceGuidsWithStataCaptions(question.ConditionExpression, model.Id);
                question.ValidationExpression =
                    expressionReplacer.ReplaceGuidsWithStataCaptions(question.ValidationExpression, model.Id);
            }

            foreach (EditGroupView group in model.Groups)
            {
                group.ConditionExpression = expressionReplacer.ReplaceGuidsWithStataCaptions(group.ConditionExpression, model.Id);
            }
        }

        private void SaveRequest(int? pageIndex, ref string sortBy, int? sortOrder, string filter)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = filter;
            this.ViewBag.SortOrder = sortOrder;

            if (sortOrder.ToBool())
            {
                sortBy = string.Format("{0} Desc", sortBy);
            }
        }

        public ActionResult LackOfPermits()
        {
            this.Error("You no longer have permission to edit this questionnaire");
            return this.RedirectToAction("Index");
        }
    }
}