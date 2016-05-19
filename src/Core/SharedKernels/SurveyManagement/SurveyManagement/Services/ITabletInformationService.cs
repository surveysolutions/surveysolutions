using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ITabletInformationService
    {
        void SaveTabletInformation(byte[] content, string androidId, UserView user);

        List<TabletInformationView> GetAllTabletInformationPackages();
        string GetFullPathToContentFile(string packageName);
        string GetFileName(string fileName, string hostName);
    }
}
