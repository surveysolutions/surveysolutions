using System.IO;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    public class LookupTableContentFile
    {
        public LookupTableContentFile(string fileName, byte[] content)
        {
            FileName = fileName;
            Content = content;
        }

        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }
}
