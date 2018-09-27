using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Services.Processing.Good;

namespace WB.Services.Export.Jobs
{
    public interface IExportJob
    {
        Task ExecuteAsync(DataExportProcessDetails args, CancellationToken cancellationToken);
    }
}
