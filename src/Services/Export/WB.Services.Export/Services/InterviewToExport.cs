using System;
using WB.Services.Export.Interview;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Services
{
    public struct InterviewToExport
    {
        public InterviewToExport(Guid id, string key, InterviewStatus status, int? assignmentId)
        {
            Id = id;
            Key = key;
            Status = status;
            AssignmentId = assignmentId;
        }

        public Guid Id { get; }

        public string Key { get; }

        public InterviewStatus Status { get; }

        public int? AssignmentId { get; }
    }
}
