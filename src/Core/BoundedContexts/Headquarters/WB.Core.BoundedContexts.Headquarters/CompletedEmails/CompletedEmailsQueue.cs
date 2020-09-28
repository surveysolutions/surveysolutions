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

        public IEnumerable<Guid> GetInterviewIdsForSend()
        {
            return storage.Query(_ => _
                .OrderBy(r => r.RequestTime)
                .Select(r => r.InterviewId));
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