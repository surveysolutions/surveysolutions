using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class LastCompletionComments : ILastCompletionComments
    {
        private readonly Dictionary<Guid, string> comments = new Dictionary<Guid, string>();

        public void Store(Guid interviewId, string comment)
        {
            comments[interviewId] = comment;
        }

        public string Get(Guid interviewId)
        {
            return comments.GetOrNull(interviewId);
        }

        public void Remove(Guid interviewId)
        {
            if (comments.ContainsKey(interviewId))
            {
                comments.Remove(interviewId);
            }
        }
    }
}
