namespace WB.Services.Export.Services.Processing
{
    public interface IDataExportFileAccessor
    {
        string GetExternalStoragePath(string name);
    }
}