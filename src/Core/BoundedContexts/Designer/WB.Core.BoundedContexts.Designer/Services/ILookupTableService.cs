using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ILookupTableService
    {
        void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string lookupTableName, string fileContent);
        void DeleteLookupTableContent(Guid questionnaireId, Guid lookupTableId);
    }
}