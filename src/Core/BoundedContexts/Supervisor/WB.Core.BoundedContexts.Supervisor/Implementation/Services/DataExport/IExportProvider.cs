namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    public interface IExportProvider<T>
    {
        bool DoExport(T items, string fileName);

        byte[] DoExportToStream(T items);
    }
}