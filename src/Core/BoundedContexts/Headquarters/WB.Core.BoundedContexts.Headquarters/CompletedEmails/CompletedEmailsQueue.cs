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
            var now10Minutes = now.AddMinutes(-10);
            var now20Minutes = now.AddMinutes(-20);
            var now30Minutes = now.AddMinutes(-30);
            var now1Hour = now.AddHours(-1);
            var now3Hours = now.AddHours(-3);
            
            return storage.Query(_ => _
                .Where(r => r.FailedCount == 0
                    || (r.FailedCount == 1 && r.RequestTime >= now10Minutes)
                    || (r.FailedCount == 2 && r.RequestTime >= now20Minutes)
                    || (r.FailedCount == 3 && r.RequestTime >= now30Minutes)
                    || (r.FailedCount == 4 && r.RequestTime >= now1Hour)
                    || (r.FailedCount >= 5 && r.RequestTime >= now3Hours)
                )
                .OrderBy(r => r.RequestTime)
                .Select(r => r.InterviewId))
                .Take(batchSize)
                .ToList();
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
            record.FailedCount++;
            storage.Store(record, interviewId);
        }
    }
}