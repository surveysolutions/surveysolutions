using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "AssignInterviewToUser")]
    public class AssignInterviewToUserCommand : CommandBase
    {
        public AssignInterviewToUserCommand(Guid interviewId, Guid userId)
        {
            this.UserId = userId;
            this.InterviewId = interviewId;
        }

        [AggregateRootId]
        public Guid InterviewId { get; set; }

        public Guid UserId { get; set; }
    }
}
