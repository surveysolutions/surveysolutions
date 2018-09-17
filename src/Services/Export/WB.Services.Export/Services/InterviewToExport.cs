using System;
using System.Collections.Generic;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;

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
            this.Entities = new List<InterviewEntity>();
        }

        public Guid Id { get; }

        public string Key { get; }
        public int ErrorsCount { get; }
        public InterviewStatus Status { get; }
        public List<InterviewEntity> Entities { get; set; }
    }
}
