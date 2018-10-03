using System;

namespace WB.Core.SharedKernels.Enumerator.Services.MapService
{
    public class MapDescription
    {
        public string MapName { set; get; }
        public string MapFileName { set; get; }
        public string MapFullPath { set; get; }
        public long Size { set; get; }
        public DateTime CreationDate { set; get; }
    }
}
