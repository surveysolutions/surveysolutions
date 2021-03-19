using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Enumerator.Native.WebInterview
{
    public class InterviewSimpleStatus
    {
        public GroupStatus SimpleStatus { get; set; }
        public int ActiveQuestionCount { get; set; }
        public int AnsweredQuestionsCount { get; set; }
    }
}
