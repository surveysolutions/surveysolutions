#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    class CompletedEmailsQueue : ICompletedEmailsQueue
    {
        private readonly IPlainStorageAccessor<CompletedEmailRecord> storage;

        public CompletedEmailsQueue(IPlainStorageAccessor<CompletedEmailRecord> storage)
        {
            this.storage = storage;
        }

        public List<Guid> GetInterviewIdsForSend(int batchSize = 100)
        {
            DateTime now = DateTime.UtcNow;

            return storage.Query(records => records
                .Where(r => r.NextAttemptAt == null || r.NextAttemptAt <= now)
                .OrderBy(r => r.RequestTime)
                .ThenBy(r => r.InterviewId)
                .Take(batchSize)
                .Select(r => r.InterviewId)
                .ToList());
        }

        public void Add(Guid interviewId)
        {
            var record = storage.GetById(interviewId);
            if (record != null)
                return;

            storage.Store(new CompletedEmailRecord()
            {
                InterviewId = interviewId,
                RequestTime = DateTime.UtcNow,
            }, interviewId);
        }

        public void Remove(Guid interviewId)
        {
            storage.Remove(interviewId);
        }

        public void MarkAsFailedToSend(Guid interviewId)
        {
            var record = storage.GetById(interviewId);
            if (record == null)
                return;
            record.FailedCount++;
            var delay = record.FailedCount switch
            {
                1 => TimeSpan.FromMinutes(10),
                2 => TimeSpan.FromMinutes(20),
                3 => TimeSpan.FromMinutes(30),
                4 => TimeSpan.FromHours(1),
                _ => TimeSpan.FromHours(3)
            };
            record.NextAttemptAt = DateTime.UtcNow.Add(delay);
            storage.Store(record, interviewId);
        }
    }
}
