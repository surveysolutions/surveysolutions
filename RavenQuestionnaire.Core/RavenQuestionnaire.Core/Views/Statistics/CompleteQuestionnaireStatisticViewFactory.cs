using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;

namespace RavenQuestionnaire.Core.Views.Statistics
{
    public class CompleteQuestionnaireStatisticViewFactory : IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
    {
        private IDocumentSession documentSession;

        public CompleteQuestionnaireStatisticViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        #region Implementation of IViewFactory<UserViewInputModel,UserView>

        public CompleteQuestionnaireStatisticView Load(CompleteQuestionnaireStatisticViewInputModel input)
        {
            CompleteQuestionnaireStatisticDocument doc =
                documentSession.Load<CompleteQuestionnaireStatisticDocument>(input.Id);
            return new CompleteQuestionnaireStatisticView(doc);
        }


        #endregion
    }
}
