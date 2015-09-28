using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.CapiInformationService
{
    public interface ICapiInformationService
    {
        Task<string> CreateInformationPackage(CancellationToken ct);
    }
}