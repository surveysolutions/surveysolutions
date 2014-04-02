using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ITabletInformationService
    {
        void SaveTabletInformation(byte[] content, string androidId, string registrationId);
        List<TabletInformationView> GetAllTabletInformationPackages();
        string GetFullPathToContentFile(string packageName);
    }
}
