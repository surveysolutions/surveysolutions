using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views.TabletInformation;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface ITabletInformationService
    {
        void SaveTabletInformation(byte[] content, string androidId, string registrationId);
        List<TabletInformationView> GetAllTabletInformationPackages();
        string GetFullPathToContentFile(string packageName);
    }
}
