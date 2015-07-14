using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface IQrBarcodeScanService
    {
        Task<ScanResult> ScanAsync();
    }
}
