using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewAnswerOnPrefilledQuestionApiView
    {
        public Guid QuestionId { get; set; }
        public string Answer { get; set; }
    }
}