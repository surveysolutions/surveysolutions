using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class RestStreamResult : IDisposable
    {
        public Stream Stream { set; get; }
        public long? ContentLength { set; get; }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
