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
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;
using QuestionnaireVersion = WB.Core.SharedKernels.DataCollection.QuestionnaireVersion;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    public class ImportController : ApiController
    {
        private readonly IQuestionnaireExportService exportService;
        private readonly IStringCompressor zipUtils;
        private readonly IMembershipUserService userHelper;
        private readonly IViewFactory<QuestionnaireListInputModel, QuestionnaireListView> viewFactory;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        public ImportController(IQuestionnaireExportService exportService,
            IStringCompressor zipUtils,
            IMembershipUserService userHelper,
            IViewFactory<QuestionnaireListInputModel, QuestionnaireListView> viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator)
        {
            this.exportService = exportService;
            this.zipUtils = zipUtils;
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
        }

        [HttpPost]
        public bool ValidateCredentials()
        {
            return this.userHelper.WebUser != null && !this.userHelper.WebUser.MembershipUser.IsLockedOut;
        }

        public QuestionnaireCommunicationPackage Questionnaire(DownloadQuestionnaireRequest request)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(request.QuestionnaireId));
            if (questionnaireView == null)
            {
                var message = String.Format("Requested questionnaire id={0} was not found", request.QuestionnaireId);
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, message));
            }

            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);

            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                var message = String.Format("Requested questionnaire id={0} cannot be processed", request.QuestionnaireId);

                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.PreconditionFailed, message));
            }

            var templateTitle = string.Format("{0}.tmpl", templateInfo.Title.ToValidFileName());

            if (templateInfo.Version > request.SupportedQuestionnaireVersion)
            {
                var message = String.Format("Requested questionnaire \"{0}\" has version {1}, but Headquarters application supports versions up to {2} only",
                        templateTitle,
                        templateInfo.Version,
                        request.SupportedQuestionnaireVersion);

                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.UpgradeRequired, message));
            }

            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireView.Source).ToArray();

            if (questoinnaireErrors.Any())
            {
                var message = String.Format("Requested questionnaire \"{0}\" has errors. Please verify and fix them on Designer.",
                        templateInfo.Title);

                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.PreconditionFailed, message));
            }

            GenerationResult generationResult;
            string resultAssembly;
            try
            {
                generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireView.Source, out resultAssembly);
            }
            catch (Exception)
            {
                generationResult = new GenerationResult()
                {
                    Success = false,
                    Diagnostics = new List<GenerationDiagnostic>() { new GenerationDiagnostic("Common verifier error", "Error", "unknown", GenerationDiagnosticSeverity.Error) }
                };
                resultAssembly = string.Empty;
            }

            if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
            {
                var message = String.Format("Requested questionnaire \"{0}\" has errors. Please verify template on Designer.",
                    templateInfo.Title);

                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.PreconditionFailed, message));
            }

            return new QuestionnaireCommunicationPackage
            {
                Questionnaire = this.zipUtils.CompressString(templateInfo.Source),
                QuestionnaireAssembly = resultAssembly
            };
        }

        public QuestionnaireListByPagesCommunicationPackage QuestionnaireList(QuestionnaireListRequest request)
        {
            var questionnaireListView = this.viewFactory.Load(
                input:
                    new QuestionnaireListInputModel
                    {

                        ViewerId = this.userHelper.WebServiceUser.UserId,
                        IsAdminMode = this.userHelper.WebServiceUser.IsAdmin,
                        Page = request.PageIndex,
                        PageSize = request.PageSize,
                        Order = request.SortOrder,
                        Filter = request.Filter
                    });

            return new QuestionnaireListByPagesCommunicationPackage()
            {
                TotalCount = questionnaireListView.TotalCount,
                Order = questionnaireListView.Order,
                Page = questionnaireListView.Page,
                PageSize = questionnaireListView.PageSize,
                Items =
                    questionnaireListView.Items.Select(
                        questionnaireListItem =>
                            new QuestionnaireListItem()
                            {
                                Id = questionnaireListItem.PublicId,
                                Title = questionnaireListItem.Title
                            }).ToList()
            };
        }
    }
}