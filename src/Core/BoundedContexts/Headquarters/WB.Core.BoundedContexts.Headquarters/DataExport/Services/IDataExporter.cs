namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExporter
    {
        void RunPendingExport();
    }
}