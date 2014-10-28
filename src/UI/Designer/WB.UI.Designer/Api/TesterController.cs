using System.Collections.Generic;
using System.Net.Http;
using Main.Core;
using Main.Core.View;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    [ApiBasicAuth]
    public class TesterController : ApiController
    {
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IQuestionnaireExportService exportService;
        private readonly ILogger logger;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;


        public TesterController(IMembershipUserService userHelper,
            IQuestionnaireHelper questionnaireHelper,
            IQuestionnaireVerifier questionnaireVerifier,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IQuestionnaireExportService exportService,
            IExpressionProcessorGenerator expressionProcessorGenerator,
            ILogger logger)
        {
            this.userHelper = userHelper;
            this.exportService = exportService;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.logger = logger;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireVerifier = questionnaireVerifier;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
        }
        
        [HttpGet]
        public HttpResponseMessage GetAllTemplates()
        {
            var user = this.userHelper.WebUser;

            if (user == null)
            {
                logger.Error("Unauthorized request to the questionnaire list");
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, TesterApiController.TesterController_ValidateCredentials_Not_authirized);
            }
            
            var questionnaireItemList = new List<QuestionnaireListItem>();
            int pageIndex = 1;
            while (true)
            {
                var questionnaireList = this.questionnaireHelper.GetQuestionnaires(
                    viewerId: user.UserId,
                    pageIndex: pageIndex);

                questionnaireItemList.AddRange(questionnaireList.Select(q => new QuestionnaireListItem()
                {
                    Id = q.Id,
                    Title = q.Title
                }).ToList());

                pageIndex++;
                if (pageIndex > questionnaireList.TotalPages)
                    break;
            }

            var questionnaireSyncPackage = new QuestionnaireListCommunicationPackage
                {
                    Items = questionnaireItemList
                };
            return Request.CreateResponse(HttpStatusCode.OK, questionnaireSyncPackage);

        }
        
        [HttpPost]
        public HttpResponseMessage ValidateCredentials()
        {
            if (this.userHelper.WebUser == null)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, TesterApiController.TesterController_ValidateCredentials_Not_authirized);

            return Request.CreateResponse(HttpStatusCode.OK, !this.userHelper.WebUser.MembershipUser.IsLockedOut);
        }

        [HttpGet]
        public HttpResponseMessage GetTemplate(Guid id)
        {
            var user = this.userHelper.WebUser;
            if (user == null)
            {
                logger.Error("Unauthorized request to the questionnaire " + id);
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, TesterApiController.TesterController_ValidateCredentials_Not_authirized);
            }

            return Request.CreateErrorResponse(HttpStatusCode.Gone, TesterApiController.TesterController_GetTemplate_You_have_an_old_version_of_application__Please_update_application_to_continue_);
        }

        [HttpGet]
        public HttpResponseMessage GetTemplate(Guid id, string maxSupportedVersion)
        {
            var user = this.userHelper.WebUser;
            if (user == null)
            {
                logger.Error("Unauthorized request to the questionnaire " + id);
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, TesterApiController.TesterController_ValidateCredentials_Not_authirized);
            }

            var questionnaireSyncPackage = new QuestionnaireCommunicationPackage();

            QuestionnaireVersion supportedQuestionnaireVersion;
            if (!QuestionnaireVersion.TryParse(maxSupportedVersion, out supportedQuestionnaireVersion))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, TesterApiController.TesterController_GetTemplate_Max_supporter_version_of_questionnaire_was_not_correctly_provided_);
            }

            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(TesterApiController.TesterController_GetTemplate_, id));
            }

            if (!ValidateAccessPermissions(questionnaireView, user.UserId))
            {
                logger.Error(String.Format("Non permitted resource was requested by user [{0}]", user.UserId));
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            }
            
            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);
            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(TesterApiController.TesterController_GetTemplate_, id));
            }

            if (templateInfo.Version > supportedQuestionnaireVersion)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, TesterApiController.TesterController_GetTemplate_You_have_an_obsolete_version_of_application__Please_update_application_to_continue_);
            }

            string resultAssembly;

            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireView.Source).ToArray();
            if (questoinnaireErrors.Any())
            {
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, TesterApiController.TesterController_GetTemplate_Questionnaire_is_invalid__Please_Verify_it_on_Designer_);
            }
            GenerationResult generationResult;
            try
            {
                generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireView.Source, out resultAssembly);
            }
            catch (Exception exc)
            {
                logger.Error("Error on assembly generation.", exc);

                generationResult = new GenerationResult()
                {
                    Success = false,
                    Diagnostics = new List<GenerationDiagnostic>() { new GenerationDiagnostic("Common verifier error", "Error", GenerationDiagnosticSeverity.Error) }
                };
                resultAssembly = string.Empty;
            }

            if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
            {
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, TesterApiController.TesterController_GetTemplate_Questionnaire_is_invalid__Please_Verify_it_on_Designer_);
            }

            var template = PackageHelper.CompressString(templateInfo.Source);
            questionnaireSyncPackage.Questionnaire = template;
            questionnaireSyncPackage.QuestionnaireAssembly = resultAssembly;

            return Request.CreateResponse(HttpStatusCode.OK, questionnaireSyncPackage);
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView, Guid currentPersonId)
        {
            if (questionnaireView.CreatedBy == currentPersonId)
                return true;

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = questionnaireView.PublicKey });
            
            bool isQuestionnaireIsSharedWithThisPerson = (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == currentPersonId);

            return isQuestionnaireIsSharedWithThisPerson;

        }
    }
}
