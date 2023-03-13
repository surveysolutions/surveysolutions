using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IMapInteractionService
    {
        Task<AreaEditResult> EditAreaAsync(EditAreaArgs args);
        Task OpenInterviewerMapDashboardAsync();
        Task OpenSupervisorMapDashboardAsync();

        public void SetLicenseKey(string key);
        public void SetApiKey(string key);
        
        bool DoesSupportMaps { get; }
    }
}
