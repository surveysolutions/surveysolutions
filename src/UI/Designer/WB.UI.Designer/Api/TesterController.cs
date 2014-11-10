using System.Collections.Generic;
using System.Net.Http;
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
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
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
        private readonly IArchiveUtils archiver;
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
            ILogger logger,
            IArchiveUtils archiver)
        {
            this.userHelper = userHelper;
            this.exportService = exportService;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.logger = logger;
            this.archiver = archiver;
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
                logger.Warn("Unauthorized request to the questionnaire list");
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
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, TesterApiController.TesterController_ValidateCredentials_Not_authirized);

            return Request.CreateResponse(HttpStatusCode.OK, !this.userHelper.WebUser.MembershipUser.IsLockedOut);
        }

        [HttpGet]
        public HttpResponseMessage GetTemplate(Guid id)
        {
            var user = this.userHelper.WebUser;
            if (user == null)
            {
                logger.Warn("Unauthorized request to the questionnaire " + id);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, TesterApiController.TesterController_ValidateCredentials_Not_authirized);
            }

            return Request.CreateErrorResponse(HttpStatusCode.Gone, TesterApiController.OldClientPleaseUpdate);
        }

        [HttpGet]
        public HttpResponseMessage GetTemplate(Guid id, string maxSupportedVersion)
        {
            var user = this.userHelper.WebUser;
            if (user == null)
            {
                logger.Warn("Unauthorized request to the questionnaire " + id);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, TesterApiController.TesterController_ValidateCredentials_Not_authirized);
            }

            QuestionnaireVersion supportedQuestionnaireVersion;
            if (!QuestionnaireVersion.TryParse(maxSupportedVersion, out supportedQuestionnaireVersion))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, TesterApiController.VersionParameterIsIncorrect);
            }

            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(TesterApiController.TemplateWasNotFound, id));
            }

            if (!ValidateAccessPermissions(questionnaireView, user.UserId))
            {
                logger.Error(String.Format("Non permitted resource was requested by user [{0}]", user.UserId));
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, TesterApiController.TesterController_ValidateCredentials_Not_authirized);
            }
            
            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);
            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(TesterApiController.TemplateWasNotFound, id));
            }

            if (templateInfo.Version > supportedQuestionnaireVersion)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                    string.Format(TesterApiController.ClientVersionLessThenDocument, supportedQuestionnaireVersion, templateInfo.Version));
            }

            string resultAssembly;
            try
            {
                if (questionnaireVerifier.Verify(questionnaireView.Source).ToArray().Any())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, TesterApiController.Questionnaire_verification_failed);
                }

                GenerationResult generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireView.Source, out resultAssembly);

                if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, TesterApiController.Questionnaire_verification_failed);
                }
            }
            catch (Exception exc)
            {
                logger.Error("Error template verification.", exc);
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, TesterApiController.Questionnaire_verification_failed);
            }
            
            var template = archiver.CompressString(templateInfo.Source);
            var questionnaireSyncPackage = new QuestionnaireCommunicationPackage
            {
                Questionnaire = template,
                QuestionnaireAssembly = resultAssembly
            };

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
