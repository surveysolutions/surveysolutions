using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class QuestionnaireKeyValueStorage : IPlainKeyValueStorage<QuestionnaireDocument>
    {
        private readonly IPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository;
        public QuestionnaireKeyValueStorage(IPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository)
        {
            this.questionnaireDocumentViewRepository = questionnaireDocumentViewRepository;
        }

        public QuestionnaireDocument GetById(string id)
        {
           var currentQuestionnaireDocument = this.questionnaireDocumentViewRepository.GetById(id);

            return currentQuestionnaireDocument?.QuestionnaireDocument;
        }

        public void Remove(string id)
        {
            this.questionnaireDocumentViewRepository.Remove(id);
        }

        public void Store(QuestionnaireDocument view, string id)
        {
            this.questionnaireDocumentViewRepository.Store(new QuestionnaireDocumentView{
                QuestionnaireDocument = view,
                Id = id
            });
        }
    }
}
