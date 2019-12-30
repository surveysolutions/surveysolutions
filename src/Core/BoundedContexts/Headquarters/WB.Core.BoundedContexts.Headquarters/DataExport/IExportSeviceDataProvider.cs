using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IExportServiceDataProvider
    {
        Dictionary<string, Dictionary<string,string>> GetServiceDataLabels();
    }
}
