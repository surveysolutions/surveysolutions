using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootConstructor(typeof(Implementation.Aggregates.Interview))]
    public class CreateInterviewCommand : CommandBase
    {
        public CreateInterviewCommand(Guid interviewId, Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
            this.InterviewId = interviewId;
        }

        [AggregateRootId]
        public Guid InterviewId { get; private set; }

        public Guid QuestionnaireId { get; private set; }
    }
}
