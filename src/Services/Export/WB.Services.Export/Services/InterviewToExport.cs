using System;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Services
{
    public struct InterviewToExport
    {
        public InterviewToExport(Guid id, string key, InterviewStatus status)
        {
            Id = id;
            Key = key;
            Status = status;
        }

        public Guid Id { get; }

        public string Key { get; }

        public InterviewStatus Status { get; }
    }
}
