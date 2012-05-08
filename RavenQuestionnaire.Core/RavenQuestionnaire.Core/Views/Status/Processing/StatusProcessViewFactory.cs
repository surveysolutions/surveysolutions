using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Status.Processing
{
    public class StatusProcessViewFactory: IViewFactory<StatusProcessViewInputModel, StatusProcessView>
    {
        private IDocumentSession documentSession;

        public StatusProcessViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusProcessView Load(StatusProcessViewInputModel input)
        {
            StatusDocument doc = documentSession.Load<StatusDocument>(input.StatusId);

            if (doc == null)
                return null;

            var statusItem = doc.Statuses.FirstOrDefault(status => status.PublicKey == input.PublicId);


            if (statusItem == null)
                return null;

            return doc == null ? null
                : new StatusProcessView(doc.Id, statusItem.Title, statusItem.IsVisible, statusItem.FlowRules,
                    statusItem.IsDefaultStuck, doc.QuestionnaireId, statusItem.PublicKey);
        }
    }
}
