using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public struct InterviewToExport
    {
        public InterviewToExport(Guid id, string key, int errorsCount, InterviewStatus status)
        {
            Id = id;
            Key = key;
            ErrorsCount = errorsCount;
            Status = status;
            Entities = new List<InterviewEntity>();
        }

        public Guid Id { get; }

        public string Key { get; }
        public int ErrorsCount { get; }
        public InterviewStatus Status { get; set; }
        public List<InterviewEntity> Entities { get; set; }
    }
}
