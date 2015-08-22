using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.UI.Interviewer.Implementations.PlainStorage;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class QuestionnaireModelRepository : IPlainKeyValueStorage<QuestionnaireModel>
    {
        private readonly SqlitePlainStorageAccessor<QuestionnaireModel> documentStore;

        private string cachedQuestionnaireId;
        private QuestionnaireModel cachedQuestionnaireModel;

        public QuestionnaireModelRepository(SqlitePlainStorageAccessor<QuestionnaireModel> documentStore)
        {
            this.documentStore = documentStore;
        }

        public QuestionnaireModel GetById(string id)
        {
            if (this.cachedQuestionnaireId != id)
            {
                this.cachedQuestionnaireModel = this.documentStore.GetById(id);
                this.cachedQuestionnaireId = id;
            }

            return this.cachedQuestionnaireModel;
        }

        public void Remove(string id)
        {
            this.documentStore.Remove(id);
        }

        public void Store(QuestionnaireModel view, string id)
        {
            this.documentStore.Store(view, id);
        }
    }
}
