using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.WebServices.Questionnaire;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.WebServices
{
    public class PublicService : IPublicService
    {
        private readonly IQuestionnaireExportService exportService;
        private readonly IMembershipUserService userHelper;
        private readonly IStringCompressor zipUtils;
        private readonly IViewFactory<QuestionnaireListInputModel, QuestionnaireListView> viewFactory;

        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;

        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;
        private readonly ILocalizationService localizationService;

        public PublicService(
            IQuestionnaireExportService exportService,
            IStringCompressor zipUtils,
            IMembershipUserService userHelper,
            IViewFactory<QuestionnaireListInputModel, QuestionnaireListView> viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            ILocalizationService localizationService)
        {
            this.exportService = exportService;
            this.zipUtils = zipUtils;
            this.userHelper = userHelper;
            this.viewFactory = viewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.questionnaireViewFactory = questionnaireViewFactory; 
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.localizationService = localizationService;
        }

        public RemoteFileInfo DownloadQuestionnaire(DownloadQuestionnaireRequest request)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(request.QuestionnaireId));
            if (questionnaireView == null)
            {
                var message = String.Format(this.localizationService.GetString("TemplateNotFound"), request.QuestionnaireId);
                throw new FaultException(message, new FaultCode("TemplateNotFound"));
            }

            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);

            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                var message = String.Format(this.localizationService.GetString("TemplateNotFound"), request.QuestionnaireId);

                throw new FaultException(message, new FaultCode("TemplateProcessingError"));
            }

            var templateTitle = string.Format("{0}.tmpl", templateInfo.Title.ToValidFileName());

            if (templateInfo.Version > request.SupportedQuestionnaireVersion)
            {
                var message = String.Format(this.localizationService.GetString("NotSupportedQuestionnaireVersion"),
                        templateTitle,
                        templateInfo.Version,
                        request.SupportedQuestionnaireVersion);

                throw new FaultException(message, new FaultCode("InconsistentVersion")); //InconsistentVersionException(message);
            }
            
            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireView.Source).ToArray();

            if (questoinnaireErrors.Any())
            {
                var message = String.Format(this.localizationService.GetString("Questionnaire_verification_failed"),
                        templateInfo.Title);

                throw new FaultException(message, new FaultCode("InvalidQuestionnaire"));
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
                var message = String.Format(this.localizationService.GetString("Questionnaire_verification_failed"),
                        templateInfo.Title);

                throw new FaultException(message, new FaultCode("InvalidQuestionnaire"));
            }

            Stream stream = this.zipUtils.Compress(templateInfo.Source);

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