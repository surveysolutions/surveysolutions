using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IDataExportService
    {
        IDictionary<string, byte[]> ExportData(Guid questionnarieid, long version, string type);
    }
}
