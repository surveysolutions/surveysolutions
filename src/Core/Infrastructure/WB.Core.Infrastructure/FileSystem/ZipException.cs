using System;

namespace WB.Core.Infrastructure.FileSystem
{
    public class ZipException : Exception
    {
        public ZipException(string message, Exception exception = null): base(message, exception) { }
    }
}
