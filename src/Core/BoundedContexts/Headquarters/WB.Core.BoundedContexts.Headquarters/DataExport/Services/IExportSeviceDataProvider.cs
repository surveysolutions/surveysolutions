using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IExportSeviceDataProvider
    {
        Dictionary<string, Dictionary<string,string>> GetServiceDataLabels();
    }
}
