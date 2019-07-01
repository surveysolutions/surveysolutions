using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.ExportProcessHandlers
{
    interface IPublisherToExternalStorage
    {
        Task PublishToExternalStorage(ExportState state, CancellationToken cancellationToken = default);
    }
}
