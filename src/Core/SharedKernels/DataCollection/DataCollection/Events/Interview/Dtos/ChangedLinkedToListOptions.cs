namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    public class ChangedLinkedToListOptions
    {
        public ChangedLinkedToListOptions(Identity questionId, decimal[] options)
        {
            this.QuestionId = questionId;
            this.Options = options;
        }

        public Identity QuestionId { get; private set; } 
        public decimal[] Options { get; private set; }
    }
}