using System;

namespace WB.Core.SharedKernels.Enumerator.Services.MapService
{
    public class ShapefileDescription
    {
        public string FullPath { set; get; }

        public string ShapefileName { set; get; }
        public string ShapefileFileName { set; get; }
        public DateTime CreationDate { set; get; }
        public long Size { set; get; }
    }
}
