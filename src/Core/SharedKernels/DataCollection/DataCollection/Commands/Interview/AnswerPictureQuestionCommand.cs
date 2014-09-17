using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerPictureQuestion")]
    public class AnswerPictureQuestionCommand: AnswerQuestionCommand
    {
        public string PictureFileName { get; private set; }

        public AnswerPictureQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string pictureFileName)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.PictureFileName = pictureFileName;
        }
    }
}
