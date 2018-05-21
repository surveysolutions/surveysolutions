using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class TextExportFile : ExportFile
    {
        public override byte[] GetFileBytes(ReportView report)
        {
            var headers = report.Headers;
            var data = report.Data;
            throw new System.NotImplementedException();
        }

        public override string MimeType => "text/plain";
        public override string FileExtension => Extension;

        public static string Extension = ".txt";
    }
}
