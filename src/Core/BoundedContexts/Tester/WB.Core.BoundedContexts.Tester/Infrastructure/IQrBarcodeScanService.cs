using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Tester.Infrastructure
{
    public interface IQrBarcodeScanService
    {
        Task<ScanResult> ScanAsync();
    }
}
