using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerAudioQuestionCommand : AnswerQuestionCommand
    {
        public string FileName { get; private set; }

        public TimeSpan Length { get; private set; }

        public AnswerAudioQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector,
            string fileName, TimeSpan length)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.FileName = fileName;
            this.Length = length;
        }
    }
}
