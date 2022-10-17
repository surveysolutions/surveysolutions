using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IMapInteractionService
    {
        Task<AreaEditResult> EditAreaAsync(EditAreaArgs args);
        Task OpenMapDashboardAsync();

        public void Init(string key);
        
        bool DoesSupportMaps { get; }
    }
}
