using System;
using System.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.ClientSettings;

namespace RavenQuestionnaire.Core.ClientSettingsProvider
{
    public interface IClientSettingsProvider
    {
        ClientSettingsView ClientSettings { get; }
    }

    public class ClientSettingsProvider : IClientSettingsProvider
    {
        private IDocumentSession session;
        private ClientSettingsView clientSettings;
        public ClientSettingsProvider(IDocumentSession session)
        {
            this.session = session;
        }

        public ClientSettingsView ClientSettings
        {
            get
            {
                if (clientSettings == null)
                {
                    var clientSettingsDoc = this.session.Query<ClientSettingsDocument>().FirstOrDefault();
                    if (clientSettingsDoc == null)
                    {
                        clientSettingsDoc = new ClientSettingsDocument() {PublicKey = Guid.NewGuid()};
                        session.Store(clientSettingsDoc);
                        session.SaveChanges();
                    }
                    clientSettings = new ClientSettingsView(clientSettingsDoc);
                }
                return clientSettings;
            }
        }
    }
}
