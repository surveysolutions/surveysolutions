#nullable enable

using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface ICompletedEmailsQueue
    {
        IEnumerable<Guid> GetInterviewIdsForSend();
        void Add(Guid interviewId);
        void Remove(Guid interviewId);
        void MarkAsFailedToSend(Guid interviewId);
    }
}