using System;
using System.Linq;
using System.Web.Configuration;
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
        private ClientSettingsView clientSettings;
        public ClientSettingsProvider()
        {
        }

        public ClientSettingsView ClientSettings
        {
            get
            {
                if (clientSettings == null)
                {
                    /*var clientSettingsDoc = this.session.Query<ClientSettingsDocument>().FirstOrDefault();
                    if (clientSettingsDoc == null)
                    {
                        clientSettingsDoc = new ClientSettingsDocument() {PublicKey = Guid.NewGuid()};
                        session.Store(clientSettingsDoc);
                        session.SaveChanges();
                    }*/
                    clientSettings =
                        new ClientSettingsView(Guid.Parse(WebConfigurationManager.AppSettings["ClientId"]));
                }
                return clientSettings;
            }
        }
    }
}
