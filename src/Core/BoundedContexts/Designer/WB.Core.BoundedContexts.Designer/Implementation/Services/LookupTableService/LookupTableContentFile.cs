using System.IO;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService
{
    public class LookupTableContentFile
    {
        public string FileName { get; set; }
        public MemoryStream Content { get; set; }
    }
}