using System;

namespace WB.Services.Export.Jobs
{
    public class ExportFileInfoView
    {
        public DateTime LastUpdateDate { get; set; }
        public double FileSize { get; set; }
        public bool HasFile { get; set; }
    }
}
