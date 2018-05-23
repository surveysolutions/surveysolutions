using System.Threading.Tasks;
using WB.Core.SharedKernels.Questionnaire.Documents;
using Area = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IAreaEditService
    {
        Task<AreaEditResult> EditAreaAsync(Area area, GeometryType? geometryType);
    }
}
