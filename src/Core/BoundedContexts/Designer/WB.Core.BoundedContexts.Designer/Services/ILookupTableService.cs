using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ILookupTableService
    {
        void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string fileContent);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
        LookupTableContent GetLookupTableContent(Guid questionnaireId, Guid lookupTableId);
        LookupTableContentFile GetLookupTableContentFile(Guid questionnaireId, Guid lookupTableId);
        Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId);
        void CloneLookupTable(Guid sourceQuestionnaireId, Guid sourceTableId, string sourceLookupTableName, Guid newQuestionnaireId, Guid newLookupTableId);
        bool IsLookupTableEmpty(Guid questionnaireId, Guid tableId, string lookupTableName);
    }
}