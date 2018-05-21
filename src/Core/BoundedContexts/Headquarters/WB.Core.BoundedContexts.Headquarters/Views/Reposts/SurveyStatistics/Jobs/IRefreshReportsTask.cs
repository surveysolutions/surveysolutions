using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Jobs
{
    public interface IRefreshReportsTask
    {
        void ScheduleRefresh();
        void RegisterJobStart(DateTime now);
        void RegisterJobCompletion(DateTime now);
        DateTime? LastRefreshTime();
        void ForceRefresh();
        RefreshReportsState GetReportState();
        void Run();
    }
}
