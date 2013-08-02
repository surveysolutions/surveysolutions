using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootConstructor(typeof(Implementation.Aggregates.Interview))]
    public class CreateInterviewCommand : InterviewCommand
    {
        public Guid QuestionnaireId { get; private set; }

        public CreateInterviewCommand(Guid interviewId, Guid userId, Guid questionnaireId)
            : base(interviewId, userId)
        {
            this.QuestionnaireId = questionnaireId;
        }
    }
}
