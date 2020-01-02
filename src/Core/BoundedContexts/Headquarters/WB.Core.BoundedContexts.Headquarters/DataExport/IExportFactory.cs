using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IExportFactory
    {
        ExportFile CreateExportFile(ExportFileType type);
    }
}
