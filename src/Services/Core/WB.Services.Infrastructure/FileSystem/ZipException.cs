using System;

namespace WB.Services.Infrastructure.FileSystem
{
    public class ZipException : Exception
    {
        public ZipException(string message, Exception exception = null): base(message, exception) { }
    }
}
