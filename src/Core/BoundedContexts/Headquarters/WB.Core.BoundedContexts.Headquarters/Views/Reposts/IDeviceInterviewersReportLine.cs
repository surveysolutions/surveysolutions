using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IDeviceInterviewersReport
    {
        Task<DeviceInterviewersReportView> LoadAsync(string filter, OrderRequestItem order, int pageNumber, int pageSize);
    }
}