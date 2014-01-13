using WB.Core.BoundedContexts.Supervisor.Views.DataExport;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    public interface IExportProvider<T>
    {
        void AddRecord(string filePath, T items);
        byte[] CreateHeader(HeaderStructureForLevel header);
    }
}