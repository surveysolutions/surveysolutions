using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerNumericIntegerQuestion")]
    public class AnswerNumericIntegerQuestionCommand : AnswerQuestionCommand
    {
        public int Answer { get; private set; }

        public AnswerNumericIntegerQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, int answer)
            : base(interviewId, userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}
