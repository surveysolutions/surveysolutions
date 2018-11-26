using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public abstract class ExportFile
    {
        public abstract byte[] GetFileBytes(ReportView reportView);
        public abstract string MimeType { get; }
        public abstract string FileExtension { get; }
    }
}
