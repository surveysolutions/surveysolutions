using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireDocumentWithLookUpTables
    {
        public QuestionnaireDocument QuestionnaireDocument { get; set; }
        public Dictionary<Guid, string> LookupTables { get; set; }
    }
}