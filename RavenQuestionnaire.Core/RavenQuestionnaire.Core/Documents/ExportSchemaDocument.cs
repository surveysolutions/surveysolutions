using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Documents
{
    public class ExportSchemaDocument
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }

        public string QuestionnaireId { set; get; }

        public Dictionary<Guid, string> QuestionnaireMapping { set; get; }
    }
}
