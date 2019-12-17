using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class QuestionnairesControllerBase : ControllerBase
    {
        protected readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireAssemblyAccessor questionnareAssemblyFileAccessor;
        protected readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository;
        private readonly ISerializer serializer;

        protected QuestionnairesControllerBase(
            IQuestionnaireAssemblyAccessor questionnareAssemblyFileAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer, 
            IQuestionnaireStorage questionnaireStorage, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireRepository)
        {
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.serializer = serializer;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireRepository = questionnaireRepository;
        }

        public virtual ActionResult<List<QuestionnaireIdentity>> List()
        {
            List<QuestionnaireIdentity> allQuestionnaireIdentities = this.questionnaireBrowseViewFactory.GetAllQuestionnaireIdentities().ToList();

            return allQuestionnaireIdentities;
        }

        public virtual ActionResult<List<string>> GetAttachments(Guid id, int version)
        {
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(id, version);

            if (questionnaireDocument == null)
                return NotFound();

            List<string> attachmentIds = questionnaireDocument.Attachments.Select(a => a.ContentId).ToList();

            return attachmentIds;
        }

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
        
        public virtual IActionResult GetAssembly(Guid id, int version)
        {
            if (!this.questionnareAssemblyFileAccessor.IsQuestionnaireAssemblyExists(id, version))
                return NotFound();

            var data = this.questionnareAssemblyFileAccessor.GetAssemblyAsByteArray(id, version);
            return File(data, "application/octet-stream");
        }

        public virtual IActionResult LogQuestionnaireAsSuccessfullyHandled(Guid id, int version) => NoContent();

        public virtual IActionResult LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version) => NoContent();
    }
}
