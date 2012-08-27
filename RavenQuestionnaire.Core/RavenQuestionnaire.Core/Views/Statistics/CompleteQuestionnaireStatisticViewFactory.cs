using System;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticViewFactory : IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        public CompleteQuestionnaireStatisticViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public CompleteQuestionnaireStatisticView Load(CompleteQuestionnaireStatisticViewInputModel input)
        {
            CompleteQuestionnaireStoreDocument doc = store.GetByGuid(Guid.Parse(input.Id));
            return new CompleteQuestionnaireStatisticView(doc);
        }

        #endregion
    }
}
