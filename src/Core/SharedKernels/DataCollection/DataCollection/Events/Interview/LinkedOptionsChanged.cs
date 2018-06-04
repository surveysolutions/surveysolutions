using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class LinkedOptionsChanged: InterviewPassiveEvent
    {
        public LinkedOptionsChanged(ChangedLinkedOptions[] changedLinkedQuestions, DateTimeOffset originDate) 
            : base(originDate)
        {
            this.ChangedLinkedQuestions = changedLinkedQuestions;
        }

        public ChangedLinkedOptions[] ChangedLinkedQuestions { get; private set; }
    }
}
