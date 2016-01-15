using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public interface IGpsLocationService
    {
        Task<GeoLocation> GetLocation(CancellationToken cancellationToken);
    }
}