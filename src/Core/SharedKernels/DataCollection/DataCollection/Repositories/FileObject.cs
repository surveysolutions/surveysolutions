using System;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public class FileObject
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}
