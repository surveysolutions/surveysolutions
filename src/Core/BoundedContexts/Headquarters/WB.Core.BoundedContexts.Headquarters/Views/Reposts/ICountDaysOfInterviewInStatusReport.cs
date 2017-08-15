using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports
{
    public interface ICountDaysOfInterviewInStatusReport
    {
        Task<CountDaysOfInterviewInStatusRow[]> LoadAsync(CountDaysOfInterviewInStatusInputModel input);
    }
}