using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IExportServiceDataProvider
    {
        Dictionary<string, Dictionary<string,string>> GetServiceDataLabels();
    }
}
