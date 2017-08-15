using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IExportFactory
    {
        ExportFile CreateExportFile(ExportFileType type);
    }
}