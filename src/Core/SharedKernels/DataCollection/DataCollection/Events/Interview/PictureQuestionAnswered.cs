using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class PictureQuestionAnswered: QuestionAnswered
    {
        public string PictureFileName { get; private set; }

        public PictureQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, string pictureFileName)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.PictureFileName = pictureFileName;
        }
    }
}
