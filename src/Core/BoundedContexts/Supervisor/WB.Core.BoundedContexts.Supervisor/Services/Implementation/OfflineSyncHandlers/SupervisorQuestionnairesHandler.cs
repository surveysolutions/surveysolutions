using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorQuestionnairesHandler : IHandleCommunicationMessage
    {
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyAccessor;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;
        private readonly ISerializer serializer;

        public SupervisorQuestionnairesHandler(
            IInterviewerQuestionnaireAccessor questionnairesAccessor, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyAccessor, 
            IPlainStorage<TranslationInstance> translationsStorage,
            ISerializer serializer)
        {
            this.questionnairesAccessor = questionnairesAccessor;
            this.questionnaireAssemblyAccessor = questionnaireAssemblyAccessor;
            this.translationsStorage = translationsStorage;
            this.serializer = serializer;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetQuestionnaireList.Request, GetQuestionnaireList.Response>(GetList);
            requestHandler.RegisterHandler<GetQuestionnaireAssemblyRequest, GetQuestionnaireAssemblyResponse>(GetQuestionnaireAssembly);
            requestHandler.RegisterHandler<GetQuestionnaireRequest, GetQuestionnaireResponse>(GetQuestionnaire);
            requestHandler.RegisterHandler<GetQuestionnaireTranslationRequest, GetQuestionnaireTranslationResponse>(GetQuestionnaireTranslation);
        }
        
        public Task<GetQuestionnaireResponse> GetQuestionnaire(GetQuestionnaireRequest arg)
        {
            var questionnaireDocument = this.questionnairesAccessor.GetQuestionnaire(arg.QuestionnaireId);
            var serializedDocument = this.serializer.Serialize(questionnaireDocument);
            return Task.FromResult(new GetQuestionnaireResponse
            {
                QuestionnaireDocument = serializedDocument
            });
        }
        
        public Task<GetQuestionnaireTranslationResponse> GetQuestionnaireTranslation(GetQuestionnaireTranslationRequest request)
        {
            var storedTranslations = this.translationsStorage.Where(x => x.QuestionnaireId == request.QuestionnaireIdentity.ToString());

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

        public Task<GetQuestionnaireList.Response> GetList(GetQuestionnaireList.Request arg)
        {
            return Task.FromResult(new GetQuestionnaireList.Response
            {
                Questionnaires = this.questionnairesAccessor.GetAllQuestionnaireIdentities()
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
