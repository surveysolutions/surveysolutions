namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    public class ChangedLinkedOptions
    {
        public ChangedLinkedOptions(Identity questionId, RosterVector[] options)
        {
            this.QuestionId = questionId;
            this.Options = options;
        }

        public Identity QuestionId { get; private set; } 
        public RosterVector[] Options { get; private set; }
    }
}