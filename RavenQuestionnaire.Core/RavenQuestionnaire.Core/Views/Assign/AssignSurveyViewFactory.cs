using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Assign
{
    public class AssignSurveyViewFactory : IViewFactory<AssignSurveyInputModel, AssignSurveyView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> docs;

        public AssignSurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> docs, IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
            this.docs = docs;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            var q = store.GetByGuid(input.CompleteQuestionnaireId);
            var doc = this.docs.GetByGuid(input.CompleteQuestionnaireId);

            return new AssignSurveyView(doc, q);
        }
        #endregion
    }
}
