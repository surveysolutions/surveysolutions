using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal interface IExportProcessHandler<TProcess> where TProcess: IDataExportProcessDetails
    {
        void ExportData(TProcess process);
    }
}
