using System.Threading.Tasks;
using WB.Core.SharedKernels.Questionnaire.Documents;
using Area = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IMapInteractionService
    {
        Task<AreaEditResult> EditAreaAsync(Area area, GeometryType? geometryType);
        Task OpenMapDashboardAsync();

        public void Init(string key);
        
        bool DoesSupportMaps { get; }
    }
}
