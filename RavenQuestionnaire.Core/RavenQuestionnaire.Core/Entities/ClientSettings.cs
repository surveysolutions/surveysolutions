using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class ClientSettings : IEntity<ClientSettingsDocument>
    {
        private ClientSettingsDocument innerDocument;
        ClientSettingsDocument IEntity<ClientSettingsDocument>.GetInnerDocument()
        {
            return innerDocument;
        }
        public ClientSettings(ClientSettingsDocument document)
        {
            this.innerDocument = document;
               // throw new InvalidOperationException("can't be bellow zero");
        }
        public ClientSettings(Guid publicKey)
        {

            innerDocument = new ClientSettingsDocument();
            innerDocument.PublicKey = publicKey;
        }
        public Guid ClientSettingsPublicKey
        {
            get { return innerDocument.PublicKey; }
        }
    }
}
