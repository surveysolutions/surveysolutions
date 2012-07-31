using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.ClientSettings
{
    public class ClientSettingsView
    {
        public ClientSettingsView(ClientSettingsDocument doc)
        {
            this.PublicKey = doc.PublicKey;
        }
        public ClientSettingsView(Guid key)
        {
            this.PublicKey = key;
        }

        public Guid PublicKey { get; set; }
    }
}
