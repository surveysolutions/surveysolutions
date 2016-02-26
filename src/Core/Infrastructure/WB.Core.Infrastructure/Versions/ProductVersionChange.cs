using System;

namespace WB.Core.Infrastructure.Versions
{
    public class ProductVersionChange
    {
        protected ProductVersionChange() { }

        public ProductVersionChange(string productVersion, DateTime updateTimeUtc)
        {
            this.ProductVersion = productVersion;
            this.UpdateTimeUtc = updateTimeUtc;
        }

        public virtual string ProductVersion { get; set; }
        public virtual DateTime UpdateTimeUtc { get; set; }
    }
}