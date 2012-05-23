using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.ClientSettings
{
    public class ClientSettingsViewFactory:IViewFactory<ClientSettingsInputModel, ClientSettingsView>
    {
          private IDocumentSession documentSession;

          public ClientSettingsViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }
          public ClientSettingsView Load(ClientSettingsInputModel input)
          {
              return new ClientSettingsView(this.documentSession.Query<ClientSettingsDocument>().FirstOrDefault());
          }
    }
}
