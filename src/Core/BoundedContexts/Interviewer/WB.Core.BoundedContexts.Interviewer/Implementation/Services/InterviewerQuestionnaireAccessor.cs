using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerQuestionnaireAccessor : IInterviewerQuestionnaireAccessor
    {
        private readonly ISerializer serializer;
        private readonly IQuestionnaireModelBuilder questionnaireModelBuilder;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IInterviewerInterviewAccessor interviewFactory;

        private readonly IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentRepository;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;

        public InterviewerQuestionnaireAccessor(
            ISerializer serializer,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentRepository,
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IInterviewerInterviewAccessor interviewFactory)
        {
            this.serializer = serializer;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelViewRepository = questionnaireModelViewRepository;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.interviewFactory = interviewFactory;
        }

        public async Task StoreQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument, bool census)
        {
            var questionnaireId = questionnaireIdentity.ToString();

            var serializedQuestionnaireDocument = await Task.Run(() => this.serializer.Deserialize<QuestionnaireDocument>(questionnaireDocument));
            var questionnaireModel = await Task.Run(() => this.questionnaireModelBuilder.BuildQuestionnaireModel(serializedQuestionnaireDocument));

            await this.questionnaireDocumentRepository.StoreAsync(new QuestionnaireDocumentView
            {
                Id = questionnaireId,
                Document = serializedQuestionnaireDocument
            });
            await this.questionnaireModelViewRepository.StoreAsync(new QuestionnaireModelView
            {
                Id = questionnaireId,
                Model = questionnaireModel
            });
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
            await this.questionnaireDocumentRepository.RemoveAsync(questionnaireId);
            await this.questionnaireModelViewRepository.RemoveAsync(questionnaireId);
            await this.questionnaireViewRepository.RemoveAsync(questionnaireId);
            await this.questionnaireAssemblyFileAccessor.RemoveAssemblyAsync(questionnaireIdentity);
        }

        private async Task DeleteInterviewsByQuestionnaireAsync(string questionnaireId)
        {
            var interviewIdsByQuestionnaire = this.interviewViewRepository.Query(
                interviews => interviews.Where(
                    interview => interview.QuestionnaireId == questionnaireId)
                    .Select(interview => interview.InterviewId)
                    .ToList());

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
            return this.questionnaireViewRepository.Query(
                questionnaires => questionnaires.Where(questionnaire => questionnaire.Census)
                    .Select(questionnaire => questionnaire.Identity)
                    .ToList());
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