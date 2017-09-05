using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports
{
    public interface ICountDaysOfInterviewInStatusReport : IAsyncReport<CountDaysOfInterviewInStatusInputModel>
    {
        Task<CountDaysOfInterviewInStatusView> LoadAsync(CountDaysOfInterviewInStatusInputModel input);
    }
}