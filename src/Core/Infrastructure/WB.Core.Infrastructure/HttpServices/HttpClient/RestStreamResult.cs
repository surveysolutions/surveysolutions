using System;
using System.IO;

namespace WB.Core.Infrastructure.HttpServices.HttpClient
{
    public class RestStreamResult : IDisposable
    {
        public Stream Stream { set; get; }
        public long? ContentLength { set; get; }
        public bool IsPartialContent { get; set; }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
