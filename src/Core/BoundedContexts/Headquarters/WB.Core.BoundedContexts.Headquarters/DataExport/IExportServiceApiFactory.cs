namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IExportServiceApiFactory
    {
        IExportServiceApi CreateClient();

    }
}