using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public struct InterviewToExport
    {
        public InterviewToExport(Guid id, string key, bool hasErrors, InterviewStatus status)
        {
            Id = id;
            Key = key;
            HasErrors = hasErrors;
            Status = status;
        }

        public Guid Id { get; }

        public string Key { get; }

        public bool HasErrors { get; }
        public InterviewStatus Status { get; set; }
    }
}