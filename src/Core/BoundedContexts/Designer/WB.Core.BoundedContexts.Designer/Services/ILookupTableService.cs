using System;
using System.IO;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ILookupTableService
    {
        void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string lookupTableName, string fileContent);
        void DeleteLookupTableContent(Guid questionnaireId, Guid lookupTableId);
        MemoryStream GetLookupTableContent(Guid questionnaireId, Guid lookupTableId);
    }
}