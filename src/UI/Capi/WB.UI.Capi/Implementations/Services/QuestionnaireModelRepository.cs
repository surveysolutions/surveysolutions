using AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Capi.Implementations.PlainStorage;

namespace WB.UI.Capi.Implementations.Services
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

            return cachedQuestionnaireModel;
        }

        public void Remove(string id)
        {
            documentStore.Remove(id);
        }

        public void Store(QuestionnaireModel view, string id)
        {
            documentStore.Store(view, id);
        }
    }
}
