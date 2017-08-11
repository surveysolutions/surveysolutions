using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports
{
    public interface ICountDaysOfInterviewInStatusReport
    {
        CountDaysOfInterviewInStatusRow[] Load(CountDaysOfInterviewInStatusInputModel input);
    }
}