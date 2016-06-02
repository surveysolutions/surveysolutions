using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ITabletInformationService
    {
        void SaveTabletInformation(byte[] content, string androidId, UserView user);

        List<TabletInformationView> GetAllTabletInformationPackages();
        string GetFullPathToContentFile(string packageName);
        string GetFileName(string fileName, string hostName);
    }
}
