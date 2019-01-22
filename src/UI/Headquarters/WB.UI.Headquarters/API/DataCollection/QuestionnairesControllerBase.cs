﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class QuestionnairesControllerBase : ApiController
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

        public virtual HttpResponseMessage List()
        {
            var allQuestionnaireIdentities = this.questionnaireBrowseViewFactory.GetAllQuestionnaireIdentities();

            var response = this.Request.CreateResponse(allQuestionnaireIdentities);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = false,
                NoCache = true
            };

            return response;
        }

        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaireAttachments)]
        public virtual HttpResponseMessage GetAttachments(Guid id, int version)
        {
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(id, version);

            if (questionnaireDocument == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var attachmentIds = questionnaireDocument.Attachments.Select(a => a.ContentId).ToList();

            var response = this.Request.CreateResponse(attachmentIds);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }

        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaire)]
        public virtual HttpResponseMessage Get(Guid id, int version, long contentVersion)
        {
            var questionnaireDocumentVersioned = this.questionnaireStorage.GetQuestionnaireDocument(id, version);
            var questionnaireBrowseItem = this.questionnaireRepository.GetById(new QuestionnaireIdentity(id, version).ToString());

            if (questionnaireDocumentVersioned == null || questionnaireBrowseItem==null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (contentVersion < questionnaireBrowseItem.QuestionnaireContentVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }

            var questionnaireDocumentVersionedSrialized = this.serializer.Serialize(questionnaireDocumentVersioned);

            var resultValue = new QuestionnaireApiView
            {
                QuestionnaireDocument = questionnaireDocumentVersionedSrialized,
                AllowCensus = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version)).AllowCensusMode
            };

            var response = this.Request.CreateResponse(resultValue);

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaireAssembly)]
        public virtual HttpResponseMessage GetAssembly(Guid id, int version)
        {
            if (!this.questionnareAssemblyFileAccessor.IsQuestionnaireAssemblyExists(id, version))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(
                        new MemoryStream(this.questionnareAssemblyFileAccessor.GetAssemblyAsByteArray(id, version)))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
        
        [WriteToSyncLog(SynchronizationLogType.QuestionnaireProcessed)]
        public virtual HttpResponseMessage LogQuestionnaireAsSuccessfullyHandled(Guid id, int version)
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
        
        [WriteToSyncLog(SynchronizationLogType.QuestionnaireAssemblyProcessed)]
        public virtual HttpResponseMessage LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version)
        {
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
