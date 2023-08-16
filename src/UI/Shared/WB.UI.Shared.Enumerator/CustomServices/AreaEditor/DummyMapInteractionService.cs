using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class DummyMapInteractionService : IMapInteractionService
    {
        public Task<AreaEditResult> EditAreaAsync(EditAreaArgs args)
        {
            throw new NotImplementedException("This functionality is not available");
        }

        public Task OpenInterviewerMapDashboardAsync()
        {
            return Task.CompletedTask;
        }

        public Task OpenSupervisorMapDashboardAsync()
        {
            return Task.CompletedTask;
        }

        public void SetLicenseKey(string key)
        {
        }

        public void SetApiKey(string key)
        {
        }

        public bool DoesSupportMaps => false;
    }
}
