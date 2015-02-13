using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ITabletInformationService
    {
        void SaveTabletInformation(byte[] content, string androidId, string registrationId);
        List<TabletInformationView> GetAllTabletInformationPackages();
        string GetFullPathToContentFile(string packageName);
        TabletLogView GetTabletLog(string deviceId);
    }
}
