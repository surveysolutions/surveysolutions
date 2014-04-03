using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
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
