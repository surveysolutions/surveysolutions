using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class LinkedToListOptionsChanged : InterviewPassiveEvent
    {
        public LinkedToListOptionsChanged(ChangedLinkedToListOptions[] changedLinkedQuestions)
        {
            this.ChangedLinkedQuestions = changedLinkedQuestions;
        }

        public ChangedLinkedToListOptions[] ChangedLinkedQuestions { get; private set; }
    }
}