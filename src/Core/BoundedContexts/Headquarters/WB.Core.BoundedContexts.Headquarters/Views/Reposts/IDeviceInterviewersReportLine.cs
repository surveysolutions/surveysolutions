using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IDeviceInterviewersReport : IAsyncReport<DeviceByInterviewersReportInputModel>
    {
        Task<DeviceInterviewersReportView> LoadAsync(DeviceByInterviewersReportInputModel input);
    }
}