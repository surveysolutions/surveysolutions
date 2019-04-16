using System;

namespace WB.Core.Infrastructure.Versions
{
    public class ProductVersionChange
    {
        public virtual string ProductVersion { get; set; }
        public virtual DateTime UpdateTimeUtc { get; set; }
    }
}
