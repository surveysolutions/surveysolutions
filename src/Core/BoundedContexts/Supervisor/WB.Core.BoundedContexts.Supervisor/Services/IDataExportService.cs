using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IDataExportService
    {
        IDictionary<string, byte[]> ExportData(Guid questionnaireId, long version, string type);
    }
}
