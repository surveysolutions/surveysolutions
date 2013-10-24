using System.Web.Security;
using Main.Core.Domain;
using Main.Core.Domain.Exceptions;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using System.Linq;
using WB.Core.GenericSubdomains.Logging;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using Main.Core.View;
    using Main.Core.View.Question;

    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Extensions;
    using WB.UI.Designer.Models;
    using WB.UI.Designer.Utils;
    using WB.UI.Designer.Views.Questionnaire;
    using WB.UI.Shared.Web.Membership;

    [CustomAuthorize]
    public class QuestionnaireController : BaseController
    {
        private readonly ICommandService commandService;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IExpressionReplacer expressionReplacer;
        private readonly ILogger logger;

        public QuestionnaireController(
            ICommandService commandService,
            IMembershipUserService userHelper,
            IQuestionnaireVerifier questionnaireVerifier,
            IQuestionnaireHelper questionnaireHelper,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IExpressionReplacer expressionReplacer,
            ILogger logger)
            : base(userHelper)
        {
            this.commandService = commandService;
            this.questionnaireVerifier = questionnaireVerifier;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.expressionReplacer = expressionReplacer;
            this.logger = logger;
        }

        public ActionResult Clone(Guid id)
        {
            QuestionnaireView model = this.GetQuestionnaire(id);
            return
                this.View(
                    new QuestionnaireCloneModel { Title = string.Format("Copy of {0}", model.Title), Id = model.PublicKey });
        }

        [HttpPost]
        public JsonResult RemoteVerification(Guid id)
        {
            var questoinnaireErrors = questionnaireVerifier.Verify(this.GetQuestionnaire(id).Source).ToArray();
            return this.Json(questoinnaireErrors);
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

                    return this.RedirectToAction("Edit", new { id = questionnaireId });
                }
                catch (Exception e)
                {
                    logger.Error("Error on questionnaire cloning.", e);

                    if (e.InnerException is DomainException)
                    {
                        this.Error(e.InnerException.Message);
                        logger.Error("Inner exception: " + e.InnerException.Message, e.InnerException);
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
                return this.RedirectToAction("Edit", new { id = questionnaireId });
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
            QuestionnaireView questionnaire = this.GetQuestionnaire(id);

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
                View(new QuestionnaireEditView(questionaire: questionnaire,
                    questionnaireSharedPersons: questionnaireSharedPersons,
                    isOwner: questionnaire.CreatedBy == UserHelper.WebUser.UserId));
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

        private void ReplaceGuidsInValidationAndConditionRules(QuestionnaireView model)
        {
            var elements = new Queue<ICompositeView>();

            foreach (ICompositeView compositeView in model.Children)
            {
                elements.Enqueue(compositeView);
            }

            while (elements.Count > 0)
            {
                ICompositeView element = elements.Dequeue();

                var question = element as QuestionView;
                if (question != null)
                {
                    question.ConditionExpression =
                        this.expressionReplacer.ReplaceGuidsWithStataCaptions(question.ConditionExpression, model.PublicKey);
                    question.ValidationExpression =
                        this.expressionReplacer.ReplaceGuidsWithStataCaptions(question.ValidationExpression, model.PublicKey);
                }

                var group = element as GroupView;
                if (group != null)
                {
                    group.ConditionExpression =
                        this.expressionReplacer.ReplaceGuidsWithStataCaptions(group.ConditionExpression, model.PublicKey);
                    foreach (ICompositeView child in element.Children)
                    {
                        elements.Enqueue(child);
                    }
                }
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