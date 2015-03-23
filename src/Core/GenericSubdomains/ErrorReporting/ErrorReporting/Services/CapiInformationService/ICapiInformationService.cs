using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService
{
    public interface ICapiInformationService
    {
        Task<string> CreateInformationPackage(CancellationToken ct);
    }
}