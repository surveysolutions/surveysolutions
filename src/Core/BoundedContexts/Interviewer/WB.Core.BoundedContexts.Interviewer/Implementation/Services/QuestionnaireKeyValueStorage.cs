using Main.Core.Documents;
using Nito.AsyncEx;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class QuestionnaireKeyValueStorage : IPlainKeyValueStorage<QuestionnaireDocument>
    {
        private readonly IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository;
        public QuestionnaireKeyValueStorage(IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository)
        {
            this.questionnaireDocumentViewRepository = questionnaireDocumentViewRepository;
        }

        public QuestionnaireDocument GetById(string id)
        {
           var currentQuestionnaireDocument = this.questionnaireDocumentViewRepository.GetById(id);

            return currentQuestionnaireDocument?.Document;
        }

        public void Remove(string id)
        {
            this.questionnaireDocumentViewRepository.Remove(id);
        }

        public void Store(QuestionnaireDocument view, string id)
        {
            this.questionnaireDocumentViewRepository.Store(new QuestionnaireDocumentView{
                Document = view,
                Id = id
            });
        }
    }
}