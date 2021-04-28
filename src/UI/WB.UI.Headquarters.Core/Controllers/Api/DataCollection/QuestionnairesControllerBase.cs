using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class QuestionnairesControllerBase : ControllerBase
    {
        protected readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireAssemblyAccessor assemblyAccessor;
        protected readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;
        private readonly ISerializer serializer;
        private readonly IWebInterviewConfigProvider interviewConfigProvider;

        protected QuestionnairesControllerBase(
            IQuestionnaireAssemblyAccessor assemblyAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer, 
            IQuestionnaireStorage questionnaireStorage, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository,
            IWebInterviewConfigProvider interviewConfigProvider)
        {
            this.assemblyAccessor = assemblyAccessor;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.serializer = serializer;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewConfigProvider = interviewConfigProvider;
        }

        public virtual ActionResult<List<QuestionnaireIdentity>> List()
        {
            List<QuestionnaireIdentity> allQuestionnaireIdentities = this.questionnaireBrowseViewFactory.GetAllQuestionnaireIdentities().ToList();

            return allQuestionnaireIdentities;
        }

        public virtual ActionResult<List<QuestionnaireIdentity>> SwitchableToWeb()
        {
            var questionnairesPermittedToSwitchToWebMode = questionnaireRepository
                .Query(_ => _.Where(q => !q.Disabled)
                    .Select(w => w.Id)
                    .ToList()
                ).Where(PermittedQuestionnaire)
                .Select(QuestionnaireIdentity.Parse)
                .ToList();

            return questionnairesPermittedToSwitchToWebMode;
        }

        private bool PermittedQuestionnaire(string questionnaireId)
        {
            var properties= this.interviewConfigProvider.Get(QuestionnaireIdentity.Parse(questionnaireId));
            return properties.Started && properties.AllowSwitchToCawiForInterviewer;
        }

        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaireAttachments)]
        public virtual ActionResult<List<string>> GetAttachments(Guid id, int version)
        {
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(id, version);

            if (questionnaireDocument == null)
                return NotFound();

            List<string> attachmentIds = questionnaireDocument.Attachments.Select(a => a.ContentId).ToList();

            return attachmentIds;
        }

        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaire)]
        public virtual ActionResult<QuestionnaireApiView> Get(Guid id, int version, long contentVersion)
        {
            var questionnaireDocumentVersioned = this.questionnaireStorage.GetQuestionnaireDocument(id, version);
            var questionnaireBrowseItem = this.questionnaireRepository.GetById(new QuestionnaireIdentity(id, version).ToString());

            if (questionnaireDocumentVersioned == null || questionnaireBrowseItem==null)
                return NotFound();

            if (contentVersion < questionnaireBrowseItem.QuestionnaireContentVersion)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired);
            }

            var questionnaireDocumentVersionedSrialized = this.serializer.Serialize(questionnaireDocumentVersioned);

            var resultValue = new QuestionnaireApiView
            {
                QuestionnaireDocument = questionnaireDocumentVersionedSrialized,
                AllowCensus = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version)).AllowCensusMode
            };

            return resultValue;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaireAssembly)]
        public virtual IActionResult GetAssembly(Guid id, int version)
        {
            if (!this.assemblyAccessor.IsQuestionnaireAssemblyExists(id, version))
                return NotFound();

            var data = this.assemblyAccessor.GetAssemblyAsByteArray(id, version);
            return File(data, "application/octet-stream");
        }

        [WriteToSyncLog(SynchronizationLogType.QuestionnaireProcessed)]
        public virtual IActionResult LogQuestionnaireAsSuccessfullyHandled(Guid id, int version) => NoContent();

        [WriteToSyncLog(SynchronizationLogType.QuestionnaireAssemblyProcessed)]
        public virtual IActionResult LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version) => NoContent();
    }
}
