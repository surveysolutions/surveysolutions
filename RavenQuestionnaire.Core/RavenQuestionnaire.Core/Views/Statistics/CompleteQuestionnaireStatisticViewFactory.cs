using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.ViewSnapshot;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticViewFactory : IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
    {
        private readonly IViewSnapshot store;

        public CompleteQuestionnaireStatisticViewFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public CompleteQuestionnaireStatisticView Load(CompleteQuestionnaireStatisticViewInputModel input)
        {
            CompleteQuestionnaireDocument doc =
                store.ReadByGuid<CompleteQuestionnaireDocument>(Guid.Parse(input.Id));
            return new CompleteQuestionnaireStatisticView(doc);
        }


        #endregion
    }
}
