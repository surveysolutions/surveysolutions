using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.UI.Designer.Resources;
using WB.UI.Designer.WebServices.Questionnaire;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.WebServices
{
    public class PublicService : IPublicService
    {
        private readonly IMembershipUserService userHelper;
        private readonly IStringCompressor zipUtils;
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IExpressionsEngineVersionService expressionsEngineVersionService;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IJsonUtils jsonUtils;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        public PublicService(
            IStringCompressor zipUtils,
            IMembershipUserService userHelper,
            IQuestionnaireListViewFactory viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator, 
            IExpressionsEngineVersionService expressionsEngineVersionService, 
            IJsonUtils jsonUtils)
        {
            this.zipUtils = zipUtils;
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.questionnaireViewFactory = questionnaireViewFactory; 
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.expressionsEngineVersionService = expressionsEngineVersionService;
            this.jsonUtils = jsonUtils;
        }

        public RemoteFileInfo DownloadQuestionnaire(DownloadQuestionnaireRequest request)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(request.QuestionnaireId));
            if (questionnaireView == null)
            {
                var message = String.Format(ErrorMessages.TemplateNotFound, request.QuestionnaireId);
                throw new FaultException(message, new FaultCode("TemplateNotFound"));
            }

            var clientSupportedQuestionnaireVersion = new Version(request.SupportedQuestionnaireVersion.Major,
                request.SupportedQuestionnaireVersion.Minor, request.SupportedQuestionnaireVersion.Patch);

            if (!expressionsEngineVersionService.IsClientVersionSupported(clientSupportedQuestionnaireVersion))
            {
                var message =
                    string.Format(ErrorMessages.ClientVersionIsNotSupported, clientSupportedQuestionnaireVersion);

                throw new FaultException(message, new FaultCode("InconsistentVersion")); //InconsistentVersionException(message);
            }
            
            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireView.Source).ToArray();

            if (questoinnaireErrors.Any())
            {
                var message = String.Format(ErrorMessages.Questionnaire_verification_failed,
                        questionnaireView.Title);

                throw new FaultException(message, new FaultCode("InvalidQuestionnaire"));
            }
            
            GenerationResult generationResult;
            string resultAssembly;
            try
            {
                generationResult =
                    this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                        questionnaireView.Source, clientSupportedQuestionnaireVersion,
                        out resultAssembly);
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
                var message = String.Format(ErrorMessages.YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate, questionnaireView.Title);

                throw new FaultException(message, new FaultCode("InvalidQuestionnaire"));
            }

            Stream stream = this.zipUtils.Compress(jsonUtils.Serialize(questionnaireView.Source));

            return new RemoteFileInfo
            {
                //FileName = templateTitle,
                Length = stream.Length,
                FileByteStream = stream,
                SupportingAssembly = resultAssembly  //move it from header to the body
            };
        }

        public void Dummy()
        {
        }

        public QuestionnaireListViewMessage GetQuestionnaireList(QuestionnaireListRequest request)
        {
            var questionnaireListViewMessage = new QuestionnaireListViewMessage(
                this.viewFactory.Load(
                    input:
                        new QuestionnaireListInputModel
                        {

                            ViewerId = this.userHelper.WebServiceUser.UserId,
                            IsAdminMode = this.userHelper.WebServiceUser.IsAdmin,
                            Page = request.PageIndex,
                            PageSize = request.PageSize,
                            Order = request.SortOrder,
                            Filter = request.Filter
                        }));
            return questionnaireListViewMessage;
        }
    }
}