using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class Status : IEntity<StatusDocument>
    {
        private StatusDocument innerDocument;
        public string StatusId
        {
            get
            {
                return innerDocument.Id;
            }
        }

        StatusDocument IEntity<StatusDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }

        public Status(StatusDocument document)
        {
            this.innerDocument = document;
        }

        public Status(string title)
        {
            innerDocument = new StatusDocument() {Title = title};
        }

        public Status(string title, Dictionary<string, List<string>> statusRoles):this(title)
        {
            innerDocument.StatusRoles = statusRoles;
        }

        public void UpdateRestrictions(Dictionary<string, List<string>> restrictions)
        {
            innerDocument.StatusRoles = restrictions;
        }


    }
}
