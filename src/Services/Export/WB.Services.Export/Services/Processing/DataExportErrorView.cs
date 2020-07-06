using System;

namespace WB.Services.Export.Services.Processing
{
    public class DataExportErrorView
    {
        public DataExportError Type { get; set; }
        public string Message { get; set; } = String.Empty;
    }
}
