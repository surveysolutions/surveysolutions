using System;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticViewFactory : IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
    {
        private IDenormalizerStorage<CompleteQuestionnaireDocument> store;

        public CompleteQuestionnaireStatisticViewFactory(IDenormalizerStorage<CompleteQuestionnaireDocument> store)
        {
            this.store = store;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public CompleteQuestionnaireStatisticView Load(CompleteQuestionnaireStatisticViewInputModel input)
        {
            CompleteQuestionnaireDocument doc = store.GetByGuid(Guid.Parse(input.Id));
            return new CompleteQuestionnaireStatisticView(doc);
        }

        #endregion
    }
}
