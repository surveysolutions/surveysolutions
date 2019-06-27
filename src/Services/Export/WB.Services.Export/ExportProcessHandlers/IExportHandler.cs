using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.ExportProcessHandlers
{

    interface IExportHandler
    {
        Task ExportDataAsync(ExportState state, CancellationToken cancellationToken);
    }
}
