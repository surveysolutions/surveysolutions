using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IMapInteractionService
    {
        Task<AreaEditResult> EditAreaAsync(EditAreaArgs args, bool supportOfflineMaps);
        Task OpenInterviewerMapDashboardAsync();
        Task OpenAssignmentMapAsync(int assignmentId);
        Task OpenSupervisorMapDashboardAsync();

        public void SetLicenseKey(string key);
        public void SetApiKey(string key);
        
        bool DoesSupportMaps { get; }
    }
}
