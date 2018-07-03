using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public class RestorePackageInfo
    {
        public string FileLocation { get; set; }
        public long FileSize { get; set; }
        public DateTime FileCreationDate { get; set; }
    }
}