namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export
{
    public class TextExportFile : ExportFile
    {
        public override byte[] GetFileBytes(string[] headers, object[][] data)
        {
            throw new System.NotImplementedException();
        }

        public override string MimeType => "text/plain";
        public override string FileExtension => Extension;

        public static string Extension = ".txt";
    }
}