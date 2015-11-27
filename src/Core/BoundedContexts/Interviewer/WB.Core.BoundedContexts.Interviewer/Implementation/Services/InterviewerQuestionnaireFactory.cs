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
    public class InterviewerQuestionnaireFactory : IInterviewerQuestionnaireFactory
    {
        private readonly ISerializer serializer;
        private readonly IQuestionnaireModelBuilder questionnaireModelBuilder;
        private readonly IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;

        public InterviewerQuestionnaireFactory(
            ISerializer serializer,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentRepository,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor)
        {
            this.serializer = serializer;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelViewRepository = questionnaireModelViewRepository;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.questionnaireDocumentRepository = questionnaireDocumentRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
        }

        public async Task StoreQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument, bool census)
        {
            var questionnaireDocumentView = new QuestionnaireDocumentView
            {
                Id = questionnaireIdentity.ToString(),
                Document = this.serializer.Deserialize<QuestionnaireDocument>(questionnaireDocument)
            };

            var questionnaireView = new QuestionnaireView
            {
                Id = questionnaireIdentity.ToString(),
                Identity = questionnaireIdentity,
                Census = census,
                Title = questionnaireDocumentView.Document.Title
            };

            var questionnaireModelView = new QuestionnaireModelView
            {
                Model = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireDocumentView.Document),
                Id = questionnaireIdentity.ToString()
            };

            await this.questionnaireDocumentRepository.StoreAsync(questionnaireDocumentView);
            await this.questionnaireViewRepository.StoreAsync(questionnaireView);
            await this.questionnaireModelViewRepository.StoreAsync(questionnaireModelView);
        }

        public async Task RemoveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await this.questionnaireDocumentRepository.RemoveAsync(questionnaireIdentity.ToString());
            await this.questionnaireModelViewRepository.RemoveAsync(questionnaireIdentity.ToString());
            await this.questionnaireViewRepository.RemoveAsync(questionnaireIdentity.ToString());
            await this.RemoveQuestionnaireAssemblyAsync(questionnaireIdentity);
        }

        public async Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            await this.questionnaireAssemblyFileAccessor.StoreAssemblyAsync(questionnaireIdentity, assembly);
        }

        public async Task RemoveQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await this.questionnaireAssemblyFileAccessor.RemoveAssemblyAsync(questionnaireIdentity);
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