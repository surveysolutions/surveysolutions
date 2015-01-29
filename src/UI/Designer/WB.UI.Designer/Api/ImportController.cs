using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Membership;
using QuestionnaireVersion = WB.Core.SharedKernels.DataCollection.QuestionnaireVersion;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    public class ImportController : ApiController
    {
        private readonly IQuestionnaireVersioner questionnaireVersioner;
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly IQuestionnaireHelper questionnaireHelper;

        public ImportController(IQuestionnaireVersioner questionnaireVersioner,
            IMembershipUserService userHelper,
            IQuestionnaireListViewFactory viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            IQuestionnaireHelper questionnaireHelper)
        {
            this.questionnaireVersioner = questionnaireVersioner;
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.questionnaireHelper = questionnaireHelper;
        }

        [HttpGet]
        public void Login() { }

        [HttpPost]
        public QuestionnaireCommunicationPackage Questionnaire(DownloadQuestionnaireRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var questionnaireView =
                questionnaireViewFactory.Load(new QuestionnaireViewInputModel(request.QuestionnaireId));
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

            var supportedClientVersion = new QuestionnaireVersion(request.SupportedVersion.Major, request.SupportedVersion.Minor, request.SupportedVersion.Patch);
            var designerEngineVersion = this.questionnaireVersioner.GetVersion(questionnaireView.Source);

            if (designerEngineVersion > supportedClientVersion)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UpgradeRequired)
                {
                    ReasonPhrase = string.Format(ErrorMessages.ClientVersionLessThenDocument, supportedClientVersion, designerEngineVersion)
                });
            }

            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireView.Source).ToArray();

            if (questoinnaireErrors.Any())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = string.Format(ErrorMessages.Questionnaire_verification_failed, questionnaireView.Title)
                });
            }

            GenerationResult generationResult;
            string resultAssembly;
            try
            {
                generationResult =
                    this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireView.Source,
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
                            new GenerationDiagnostic("Common verifier error", "Error", "unknown",
                                GenerationDiagnosticSeverity.Error)
                        }
                };
                resultAssembly = string.Empty;
            }

            if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
                {
                    ReasonPhrase = string.Format(ErrorMessages.Questionnaire_verification_failed, questionnaireView.Title)
                });
            }

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = questionnaireView.Source,
                QuestionnaireAssembly = resultAssembly
            };
        }

        [HttpPost]
        public PagedQuestionnaireCommunicationPackage PagedQuestionnaireList(QuestionnaireListRequest request)
        {
            if(request == null) throw new ArgumentNullException("request");

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

        [HttpGet]
        public QuestionnaireListCommunicationPackage QuestionnaireList()
        {
            var questionnaireItemList = new List<QuestionnaireListItem>();
            int pageIndex = 1;

            var engineVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion();
            var supportedVersion = new Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion()
            {
                Major = engineVersion.Major,
                Minor = engineVersion.Minor,
                Patch = engineVersion.Patch
            };

            while (true)
            {
                var questionnaireList =
                    this.questionnaireHelper.GetQuestionnaires(viewerId: this.userHelper.WebUser.UserId,
                        pageIndex: pageIndex);

                questionnaireItemList.AddRange(
                    questionnaireList.Select(q => new QuestionnaireListItem()
                    {
                        Id = q.Id,
                        Title = q.Title,
                        LastModifiedDate = q.LastEntryDate,
                        OwnerName = q.Owner,
                        Version = supportedVersion
                    }).ToList());

                pageIndex++;
                if (pageIndex > questionnaireList.TotalPages)
                    break;
            }

            return new QuestionnaireListCommunicationPackage {Items = questionnaireItemList};
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