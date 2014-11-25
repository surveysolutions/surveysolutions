using System;

namespace WB.Core.Infrastructure
{
    public class ReadSideRepositoryWriterError
    {
        public DateTime ErrorDate { get; set; }
        public string ErrorMessage { get; set; }
        public string InnerException { get; set; }
    }
}