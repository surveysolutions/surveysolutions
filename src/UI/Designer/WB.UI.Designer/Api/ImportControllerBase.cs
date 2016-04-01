using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    public class ImportControllerBase : ApiController
    {
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireHelper questionnaireHelper;
        protected readonly IDesignerEngineVersionService engineVersionService;

        public ImportControllerBase(
            IMembershipUserService userHelper,
            IQuestionnaireListViewFactory viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireHelper questionnaireHelper, 
            IDesignerEngineVersionService engineVersionService)
        {
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireHelper = questionnaireHelper;
            this.engineVersionService = engineVersionService;
        }

        public virtual void Login() { }

        protected string GetQuestionnaireAssemblyOrThrow(QuestionnaireView questionnaireView, Version questionnaireContentVersion)
        {
            GenerationResult generationResult;
            string resultAssembly;
            try
            {
                generationResult =
                    this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                        questionnaireView.Source, questionnaireContentVersion,
                        out resultAssembly);
            }
            catch (Exception)
            {
                generationResult = new GenerationResult()
                {
                    Success = false,
                    Diagnostics =
                        new List<GenerationDiagnostic>()
                        {
                            new GenerationDiagnostic("Common verifier error", "unknown",
                                GenerationDiagnosticSeverity.Error)
                        }
                };
                resultAssembly = string.Empty;
            }

            if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired)
                {
                    ReasonPhrase =
                        string.Format(
                            ErrorMessages
                                .YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                            questionnaireView.Title)
                });
            }
            return resultAssembly;
        }

        protected void CheckInvariantsAndThrowIfInvalid(DownloadQuestionnaireRequest request, QuestionnaireView questionnaireView)
        {
            var supportedClientVersion = new Version(request.SupportedVersion.Major,
                request.SupportedVersion.Minor,
                request.SupportedVersion.Patch);

            if (!this.engineVersionService.IsClientVersionSupported(supportedClientVersion))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired)
                {
                    ReasonPhrase = string.Format(ErrorMessages.OldClientPleaseUpdate, supportedClientVersion)
                });
            }

            if (
                !this.engineVersionService.IsQuestionnaireDocumentSupportedByClientVersion(questionnaireView.Source,
                    supportedClientVersion))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired)
                {
                    ReasonPhrase =
                        string.Format(
                            ErrorMessages
                                .YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                            questionnaireView.Title)
                });
            }

            var questionnaireErrors = this.questionnaireVerifier.CheckForErrors(questionnaireView.Source).ToArray();

            if (questionnaireErrors.Any(x => x.MessageLevel > VerificationMessageLevel.Warning))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = ErrorMessages.Questionnaire_verification_failed
                });
            }
        }

        protected QuestionnaireView GetQuestionnaireViewOrThrow(DownloadQuestionnaireRequest request)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(request.QuestionnaireId));
            if (questionnaireView == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = string.Format(ErrorMessages.TemplateNotFound, request.QuestionnaireId)
                });
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = ErrorMessages.User_Not_authorized
                });
            }
            return questionnaireView;
        }

        public virtual PagedQuestionnaireCommunicationPackage PagedQuestionnaireList(QuestionnaireListRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var questionnaireListView = this.viewFactory.Load(
                new QuestionnaireListInputModel
                {

                    ViewerId = this.userHelper.WebUser.UserId,
                    IsAdminMode = this.userHelper.WebUser.IsAdmin,
                    Page = request.PageIndex,
                    PageSize = request.PageSize,
                    Order = request.SortOrder,
                    Filter = request.Filter
                });

            return new PagedQuestionnaireCommunicationPackage()
            {
                TotalCount = questionnaireListView.TotalCount,
                Order = questionnaireListView.Order,
                Page = questionnaireListView.Page,
                PageSize = questionnaireListView.PageSize,
                Items = questionnaireListView.Items.Select(questionnaireListItem =>
                    new QuestionnaireListItem()
                    {
                        Id = questionnaireListItem.PublicId,
                        Title = questionnaireListItem.Title
                    }).ToList()
            };
        }

        public virtual QuestionnaireListCommunicationPackage QuestionnaireList()
        {
            var questionnaireItemList = new List<QuestionnaireListItem>();
            int pageIndex = 1;
            while (true)
            {
                var questionnaireList =
                    this.questionnaireHelper.GetQuestionnaires(
                        viewerId: this.userHelper.WebUser.UserId,
                        isAdmin: this.userHelper.WebUser.IsAdmin,
                        pageIndex: pageIndex);

                questionnaireItemList.AddRange(
                    questionnaireList.Select(q => new QuestionnaireListItem() { Id = q.Id, Title = q.Title }).ToList());

                pageIndex++;
                if (pageIndex > questionnaireList.TotalPages)
                    break;
            }

            return new QuestionnaireListCommunicationPackage { Items = questionnaireItemList };
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.CreatedBy == this.userHelper.WebUser.UserId)
                return true;

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = questionnaireView.PublicKey });

            return (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == this.userHelper.WebUser.UserId);
        }
    }
}