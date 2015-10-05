using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Interviewer.Implementations.PlainStorage;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class QuestionnaireModelRepository : IPlainKeyValueStorage<QuestionnaireModel>
    {
        private readonly SqlitePlainStorageAccessor<QuestionnaireModel> documentStore;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private string cachedQuestionnaireId;
        private QuestionnaireModel cachedQuestionnaireModel;

        public QuestionnaireModelRepository(
            SqlitePlainStorageAccessor<QuestionnaireModel> documentStore, 
            IPlainQuestionnaireRepository questionnaireRepository)
        {
            this.documentStore = documentStore;
            this.questionnaireRepository = questionnaireRepository;
        }

        public QuestionnaireModel GetById(string id)
        {
            if (this.cachedQuestionnaireId != id)
            {
                this.cachedQuestionnaireModel = this.documentStore.GetById(id);

                FixCompleteScreenIfItWasStoredByPreviousVersion(cachedQuestionnaireModel, id);

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

        private void FixCompleteScreenIfItWasStoredByPreviousVersion(QuestionnaireModel model, string id)
        {
            if (model == null || model.GroupsHierarchy == null || !model.GroupsHierarchy.Any())
                return;

            var questionnaireId = QuestionnaireIdentity.Parse(id);
            var lastGroupInHierarchy = model.GroupsHierarchy.Last();

            if (lastGroupInHierarchy.Title != UIResources.Interview_Complete_Screen_Title || lastGroupInHierarchy.Children.Any())
                return;

            model.GroupsHierarchy.Remove(lastGroupInHierarchy);

            documentStore.Store(model, model.Id);

            var questionnaireDocument = questionnaireRepository.GetQuestionnaireDocument(
                questionnaireId.QuestionnaireId, questionnaireId.Version);
            if (questionnaireDocument == null)
                return;

            var groupInQuestionnaireDocument =
                questionnaireDocument.Children.OfType<IGroup>().FirstOrDefault(g => g.PublicKey == lastGroupInHierarchy.Id);
            
            if (groupInQuestionnaireDocument == null)
                return;

            questionnaireDocument.Children.Remove(groupInQuestionnaireDocument);
            questionnaireRepository.StoreQuestionnaire(questionnaireId.QuestionnaireId, questionnaireId.Version,
                questionnaireDocument);
        }
    }
}
