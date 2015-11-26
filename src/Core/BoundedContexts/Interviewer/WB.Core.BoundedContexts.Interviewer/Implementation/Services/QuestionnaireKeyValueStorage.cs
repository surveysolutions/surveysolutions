using Main.Core.Documents;
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
            return this.questionnaireDocumentViewRepository.GetById(id).Document;
        }

        public void Remove(string id)
        {
            throw new System.NotImplementedException();
        }

        public void Store(QuestionnaireDocument view, string id)
        {
            throw new System.NotImplementedException();
        }
    }
}