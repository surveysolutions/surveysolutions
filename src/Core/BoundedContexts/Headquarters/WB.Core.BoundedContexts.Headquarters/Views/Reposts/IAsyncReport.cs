using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface IAsyncReport<in T>
    {
        Task<ReportView> GetReportAsync(T model);
    }
}