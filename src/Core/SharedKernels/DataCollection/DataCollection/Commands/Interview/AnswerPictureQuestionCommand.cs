using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerPictureQuestionCommand: AnswerQuestionCommand
    {
        public string PictureFileName { get; private set; }

        public AnswerPictureQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, string pictureFileName)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.PictureFileName = pictureFileName;
        }
    }
}
