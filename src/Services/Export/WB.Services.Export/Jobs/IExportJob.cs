using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.Jobs
{
    public interface IExportJob
    {
        Task ExecuteAsync(DataExportProcessArgs args, CancellationToken cancellationToken);
    }
}
