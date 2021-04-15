namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public class InterviewSimpleStatus
    {
        public GroupStatus Status { get; set; }
        public SimpleGroupStatus SimpleStatus { get; set; }
        public int ActiveQuestionCount { get; set; }
        public int AnsweredQuestionsCount { get; set; }
    }
}
