using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class LinkedToListOptionsChanged : InterviewPassiveEvent
    {
        public LinkedToListOptionsChanged(ChangedLinkedToListOptions[] changedLinkedQuestions, DateTimeOffset originDate) 
            : base(originDate)
        {
            this.ChangedLinkedQuestions = changedLinkedQuestions;
        }

        public ChangedLinkedToListOptions[] ChangedLinkedQuestions { get; private set; }
    }
}
