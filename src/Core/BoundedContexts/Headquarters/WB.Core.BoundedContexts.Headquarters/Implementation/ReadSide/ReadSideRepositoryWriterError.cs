using System;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide
{
    public class ReadSideRepositoryWriterError
    {
        public DateTime ErrorTime { get; set; }
        public string ErrorMessage { get; set; }
        public string InnerException { get; set; }
    }
}