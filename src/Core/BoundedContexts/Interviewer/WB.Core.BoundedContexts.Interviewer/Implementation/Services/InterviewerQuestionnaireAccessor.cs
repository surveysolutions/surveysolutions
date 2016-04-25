using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerQuestionnaireAccessor : IInterviewerQuestionnaireAccessor
    {
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IInterviewerInterviewAccessor interviewFactory;

        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository;

        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocuments;
        private readonly IOptionsRepository optionsRepository;

        public InterviewerQuestionnaireAccessor(
            IJsonAllTypesSerializer synchronizationSerializer,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IInterviewerInterviewAccessor interviewFactory, 
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocuments, 
            IOptionsRepository optionsRepository)
        {
            this.synchronizationSerializer = synchronizationSerializer;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.interviewFactory = interviewFactory;
            this.questionnaireDocuments = questionnaireDocuments;
            this.optionsRepository = optionsRepository;
        }

        public async Task StoreQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument, bool census)
        {
            var questionnaireId = questionnaireIdentity.ToString();

            var serializedQuestionnaireDocument = await Task.Run(() => this.synchronizationSerializer.Deserialize<QuestionnaireDocument>(questionnaireDocument));
            serializedQuestionnaireDocument.ParseCategoricalQuestionOptions();
           
            await optionsRepository.StoreQuestionOptionsForQuestionnaireAsync(questionnaireIdentity, serializedQuestionnaireDocument);

            await Task.Run(() => this.plainQuestionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                        questionnaireIdentity.Version, serializedQuestionnaireDocument));

            await this.questionnaireViewRepository.StoreAsync(new QuestionnaireView
            {
                Id = questionnaireId,
                Identity = questionnaireIdentity,
                Census = census,
                Title = serializedQuestionnaireDocument.Title
            });
        }

        public async Task RemoveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireId = questionnaireIdentity.ToString();

            await this.DeleteInterviewsByQuestionnaireAsync(questionnaireId);
            this.plainQuestionnaireRepository.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            await this.questionnaireViewRepository.RemoveAsync(questionnaireId);
            await this.questionnaireAssemblyFileAccessor.RemoveAssemblyAsync(questionnaireIdentity);
            await optionsRepository.RemoveOptionsForQuestionnaireAsync(questionnaireIdentity);
        }

        private async Task DeleteInterviewsByQuestionnaireAsync(string questionnaireId)
        {
            var interviewIdsByQuestionnaire = this.interviewViewRepository
                .Where(interview => interview.QuestionnaireId == questionnaireId)
                .Select(interview => interview.InterviewId);

            foreach (var interviewId in interviewIdsByQuestionnaire)
            {
                await this.interviewFactory.RemoveInterviewAsync(interviewId);
            }
        }

        public async Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            await this.questionnaireAssemblyFileAccessor.StoreAssemblyAsync(questionnaireIdentity, assembly);
        }

        public List<QuestionnaireIdentity> GetCensusQuestionnaireIdentities()
        {
            return this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census)
                    .Select(questionnaire => questionnaire.Identity)
                    .ToList();
        }

        public List<QuestionnaireIdentity> GetAllQuestionnaireIdentities()
        {
            return this.questionnaireViewRepository.Where(x => true)
                 .Select(questionnaire => questionnaire.Identity)
                 .ToList();
        }

        public bool IsAttachmentUsedAsync(string contentId)
        {
            var questionnaireDocumentViews = this.questionnaireDocuments.LoadAll().Where(x => !x.Document.IsDeleted).ToList();
            return questionnaireDocumentViews.Count > 0 && questionnaireDocumentViews.Any(x => x.Document.Attachments.Any(a => a.ContentId == contentId));
        }

        public bool IsQuestionnaireExists(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.questionnaireViewRepository.GetById(questionnaireIdentity.ToString()) != null;
        }

        public bool IsQuestionnaireAssemblyExists(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(
                questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
        }
    }
}