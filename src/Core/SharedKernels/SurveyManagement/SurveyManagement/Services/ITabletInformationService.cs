using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ITabletInformationService
    {
        void SaveTabletInformation(byte[] content, string androidId, string registrationId, UserView user);

        List<TabletInformationView> GetAllTabletInformationPackages();
        List<TabletInformationView> GetAllTabletInformationPackages(int pageSize);
        string GetFullPathToContentFile(string packageName);
        string GetPackageNameWithoutRegistrationId(string packageName);
    }
}
