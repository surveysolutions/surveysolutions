using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorQuestionnairesHandler : IHandleCommunicationMessage
    {
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyAccessor;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;
        private readonly IPlainStorage<RawQuestionnaireDocumentView> rawQuestionnaireDocumentStorage;
        private readonly IPlainStorage<DeletedQuestionnaire> deletedQuestionnairesStorage;

        public SupervisorQuestionnairesHandler(
            IInterviewerQuestionnaireAccessor questionnairesAccessor,
            IQuestionnaireAssemblyAccessor questionnaireAssemblyAccessor,
            IPlainStorage<TranslationInstance> translationsStorage,
            IPlainStorage<RawQuestionnaireDocumentView> rawQuestionnaireDocumentStorage,
            IAttachmentContentStorage attachmentContentStorage, 
            IPlainStorage<DeletedQuestionnaire> deletedQuestionnairesStorage)
        {
            this.questionnairesAccessor = questionnairesAccessor;
            this.questionnaireAssemblyAccessor = questionnaireAssemblyAccessor;
            this.translationsStorage = translationsStorage;
            this.rawQuestionnaireDocumentStorage = rawQuestionnaireDocumentStorage;
            this.attachmentContentStorage = attachmentContentStorage;
            this.deletedQuestionnairesStorage = deletedQuestionnairesStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(GetList);
            requestHandler.RegisterHandler<GetQuestionnaireAssemblyRequest, GetQuestionnaireAssemblyResponse>(GetQuestionnaireAssembly);
            requestHandler.RegisterHandler<GetQuestionnaireRequest, GetQuestionnaireResponse>(GetQuestionnaire);
            requestHandler.RegisterHandler<GetQuestionnaireTranslationRequest, GetQuestionnaireTranslationResponse>(GetQuestionnaireTranslation);
            requestHandler.RegisterHandler<GetAttachmentContentsRequest, GetAttachmentContentsResponse>(GetAttachmentContents);
            requestHandler.RegisterHandler<GetAttachmentContentRequest, GetAttachmentContentResponse>(GetAttachmentContent);
        }

        private Task<GetAttachmentContentResponse> GetAttachmentContent(GetAttachmentContentRequest request)
        {
            var content = this.attachmentContentStorage.GetContent(request.ContentId);
            var meta = this.attachmentContentStorage.GetMetadata(request.ContentId);

            return Task.FromResult(new GetAttachmentContentResponse
            {
                Content = new AttachmentContent
                {
                    Content = content,
                    ContentType = meta.ContentType,
                    Id = meta.Id,
                    Size = meta.Size
                }
            });
        }

        private Task<GetAttachmentContentsResponse> GetAttachmentContents(GetAttachmentContentsRequest request)
        {
            var questionnaire = this.questionnairesAccessor.GetQuestionnaire(request.QuestionnaireIdentity);

            return Task.FromResult(new GetAttachmentContentsResponse
            {
                AttachmentContents = questionnaire.Attachments.Select(a => a.ContentId).ToList()
            });
        }

        public Task<GetQuestionnaireResponse> GetQuestionnaire(GetQuestionnaireRequest arg)
        {
            var serializedDocument = this.rawQuestionnaireDocumentStorage.GetById(arg.QuestionnaireId.ToString());
            
            return Task.FromResult(new GetQuestionnaireResponse
            {
                QuestionnaireDocument = serializedDocument.Document
            });
        }

        public Task<GetQuestionnaireTranslationResponse> GetQuestionnaireTranslation(GetQuestionnaireTranslationRequest request)
        {
            var identity = request.QuestionnaireIdentity.ToString();
            var storedTranslations = this.translationsStorage.Where(x => x.QuestionnaireId == identity);

            return Task.FromResult(new GetQuestionnaireTranslationResponse
            {
                Translations = storedTranslations.Select(tr => new TranslationDto
                {
                    Type = tr.Type,
                    Value = tr.Value,
                    QuestionnaireEntityId = tr.QuestionnaireEntityId,
                    TranslationId = tr.TranslationId,
                    TranslationIndex = tr.TranslationIndex
                }).ToList()
            });
        }

        public Task<GetQuestionnaireListResponse> GetList(GetQuestionnaireListRequest arg)
        {
            var deleted = deletedQuestionnairesStorage.LoadAll().Select(q => QuestionnaireIdentity.Parse(q.Id));

            var response = this.questionnairesAccessor.GetAllQuestionnaireIdentities()
                .Union(arg.Questionnaires)
                .Except(deleted).ToList();

            return Task.FromResult(new GetQuestionnaireListResponse
            {
                Questionnaires = response
            });
        }

        public Task<GetQuestionnaireAssemblyResponse> GetQuestionnaireAssembly(GetQuestionnaireAssemblyRequest request)
        {
            var assembly = this.questionnaireAssemblyAccessor.GetAssemblyAsByteArray(
                request.QuestionnaireId.QuestionnaireId, request.QuestionnaireId.Version);

            return Task.FromResult(new GetQuestionnaireAssemblyResponse
            {
                Content = assembly
            });
        }
    }
}
