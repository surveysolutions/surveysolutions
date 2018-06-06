using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Jobs
{
    public interface IRefreshReportsTask
    {
        Task ScheduleRefresh();
        void RegisterJobStart(DateTime now);
        void RegisterJobCompletion(DateTime now);
        DateTime? LastRefreshTime();
        Task ForceRefresh();
        Task<RefreshReportsState> GetReportState();
        Task Run();
    }
}
