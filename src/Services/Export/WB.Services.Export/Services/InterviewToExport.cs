using System;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Services
{
    public struct InterviewToExport
    {
        public InterviewToExport(Guid id, string key, int errorsCount, InterviewStatus status)
        {
            Id = id;
            Key = key;
            ErrorsCount = errorsCount;
            Status = status;
        }

        public Guid Id { get; }

        public string Key { get; }
        public int ErrorsCount { get; }
        public InterviewStatus Status { get; }
    }
}