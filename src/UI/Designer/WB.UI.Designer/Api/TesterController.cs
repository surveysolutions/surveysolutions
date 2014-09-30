﻿using System.Collections.Generic;
using Main.Core;
using Main.Core.View;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
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
            ILogger logger)
        {
            this.userHelper = userHelper;
            this.exportService = exportService;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.logger = logger;
            this.questionnaireHelper = questionnaireHelper;
            this.questionnaireVerifier = questionnaireVerifier;

            //inject it
            this.expressionProcessorGenerator = new QuestionnireExpressionProcessorGenerator();
        }
        
        [HttpGet]
        public QuestionnaireListCommunicationPackage GetAllTemplates()
        {
            var user = this.userHelper.WebUser;

            if (user == null)
            {
                logger.Error("Unauthorized request to the questionnaire list");
                throw new HttpStatusException(HttpStatusCode.Forbidden);
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

            return questionnaireSyncPackage;
        }
        
        [HttpPost]
        public bool ValidateCredentials()
        {
            if (this.userHelper.WebUser == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            if (this.userHelper.WebUser.MembershipUser.IsLockedOut)
                return false;

            return true;
        }

        [HttpGet]
        public QuestionnaireCommunicationPackage GetTemplate(Guid id)
        {
            var user = this.userHelper.WebUser;
            if (user == null)
            {
                logger.Error("Unauthorized request to the questionnaire " + id);
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            }

            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            if (!ValidateAccessPermissions(questionnaireView, user.UserId))
            {
                logger.Error(String.Format("Non permitted resource was requested by user [{0}]", user.UserId));
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            }

            var questionnaireSyncPackage = new QuestionnaireCommunicationPackage();

            string resultAssembly;

            var questoinnaireErrors = questionnaireVerifier.Verify(questionnaireView.Source).ToArray();
            if (questoinnaireErrors.Any())
            {
                questionnaireSyncPackage.IsErrorOccured = true;
                questionnaireSyncPackage.ErrorMessage = "Questionnaire is invalid. Please Verify it on Designer.";

                return questionnaireSyncPackage;
            }
            else
            {
                GenerationResult generationResult;
                try
                {
                    generationResult = this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireView.Source, out resultAssembly);
                }
                catch (Exception)
                {
                    generationResult = new GenerationResult()
                    {
                        Success = false,
                        Diagnostics = new List<GenerationDiagnostic>() { new GenerationDiagnostic("Common verifier error", "Error", GenerationDiagnosticSeverity.Error) }
                    };
                    resultAssembly = string.Empty;
                }

                if (!generationResult.Success || String.IsNullOrWhiteSpace(resultAssembly))
                {
                    questionnaireSyncPackage.IsErrorOccured = true;
                    questionnaireSyncPackage.ErrorMessage = "Questionnaire is invalid. Please Verify it on Designer.";

                    return questionnaireSyncPackage;
                }
            }
            

            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);
            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                questionnaireSyncPackage.IsErrorOccured = true;
                questionnaireSyncPackage.ErrorMessage = "Questionnaire was not found.";

                return questionnaireSyncPackage;
            }

            var template = PackageHelper.CompressString(templateInfo.Source);
            questionnaireSyncPackage.Questionnaire = template;
            questionnaireSyncPackage.QuestionnaireAssembly = resultAssembly;

            return questionnaireSyncPackage;
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
