using System;
using System.Web.Configuration;
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
                    clientSettings =
                        new ClientSettingsView(Guid.Parse(WebConfigurationManager.AppSettings["ClientId"]));
                }
                return clientSettings;
            }
        }
    }
}
