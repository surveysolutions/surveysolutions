using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ILookupTableService
    {
        void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string lookupTableName, string fileContent);
        void DeleteLookupTableContent(Guid questionnaireId, Guid lookupTableId);
        LookupTableContentFile GetLookupTableContentFile(Guid questionnaireId, Guid lookupTableId);
        Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId);
    }
}