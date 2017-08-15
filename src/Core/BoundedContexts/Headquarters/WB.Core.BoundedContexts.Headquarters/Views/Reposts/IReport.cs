using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IReport<in T>
    {
        ReportView GetReport(T model);
    }
}