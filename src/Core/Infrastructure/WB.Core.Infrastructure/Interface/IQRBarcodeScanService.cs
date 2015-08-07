using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Tester.Infrastructure
{
    public interface IQRBarcodeScanService
    {
        Task<ScanResult> ScanAsync();
    }
}
