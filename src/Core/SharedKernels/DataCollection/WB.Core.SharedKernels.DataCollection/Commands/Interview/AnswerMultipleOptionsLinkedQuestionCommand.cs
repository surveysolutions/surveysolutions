using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerMultipleOptionsLinkedQuestion")]
    public class AnswerMultipleOptionsLinkedQuestionCommand: AnswerQuestionCommand
    {
        public decimal[][] SelectedPropagationVectors { get; private set; }

        public AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedPropagationVectors = selectedPropagationVectors;
        }
    }
}
