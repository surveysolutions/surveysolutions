using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IDeviceInterviewersReport
    {
        Task<DeviceInterviewersReportView> LoadAsync(string filter, int pageNumber, int pageSize);
    }
}