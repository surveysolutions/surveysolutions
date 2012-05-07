using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusViewFactory: IViewFactory<StatusViewInputModel, StatusView>
    {
        private IDocumentSession documentSession;


        public StatusViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        /// <summary>
        /// Loads Status from Repository.
        /// Status has more priority.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public StatusView Load(StatusViewInputModel input)
        {
            StatusDocument doc = null;

            if (!string.IsNullOrEmpty(input.StatusId))
                doc = documentSession.Load<StatusDocument>(input.StatusId);

            else if(!string.IsNullOrEmpty(input.QId))
                doc = documentSession.Query<StatusDocument>().FirstOrDefault(u => u.QuestionnaireId == input.QId);

            if (doc == null)
                return null; //no satelite status document 
           
            return new StatusView(doc);
        }
    }
}
