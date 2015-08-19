using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class PictureQuestionAnswered: QuestionAnswered
    {
        public string PictureFileName { get; private set; }

        public PictureQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc, string pictureFileName)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.PictureFileName = pictureFileName;
        }
    }
}
