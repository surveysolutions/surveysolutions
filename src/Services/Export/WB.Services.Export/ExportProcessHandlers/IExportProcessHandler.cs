using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal interface IExportProcessHandler<TProcess> where TProcess: DataExportProcessArgs
    {
        Task ExportDataAsync(TProcess process, CancellationToken cancellationToken);
    }
}
