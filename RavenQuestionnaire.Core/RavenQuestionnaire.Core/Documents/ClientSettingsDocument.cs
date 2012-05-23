using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Documents
{
    public class ClientSettingsDocument
    {
        public string Id { get; set; }
        public Guid PublicKey { get; set; }
    }
}
