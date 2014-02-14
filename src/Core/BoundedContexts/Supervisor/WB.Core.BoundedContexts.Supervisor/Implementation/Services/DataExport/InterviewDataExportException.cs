using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    public class InterviewDataExportException : Exception {
        public InterviewDataExportException() {}

        public InterviewDataExportException(string message)
            : base(message) {}

        public InterviewDataExportException(string message, Exception innerException)
            : base(message, innerException) {}

        protected InterviewDataExportException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}
