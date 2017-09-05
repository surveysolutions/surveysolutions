using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public abstract class ExportFile
    {
        public abstract byte[] GetFileBytes(string[] headers, object[][] data);
        public abstract string MimeType { get; }
        public abstract string FileExtension { get; }
    }
}