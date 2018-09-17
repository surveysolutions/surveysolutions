namespace WB.Services.Export.Services.Processing
{
    class DataExportFileAccessor : IDataExportFileAccessor
    {
        public string GetExternalStoragePath(string name) => $"export/" + name;
    }
}