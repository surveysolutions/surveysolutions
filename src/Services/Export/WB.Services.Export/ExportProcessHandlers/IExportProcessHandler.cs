using System.Threading.Tasks;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal interface IExportProcessHandler<TProcess> where TProcess: IDataExportProcessDetails
    {
        Task ExportDataAsync(TProcess process);
    }
}
