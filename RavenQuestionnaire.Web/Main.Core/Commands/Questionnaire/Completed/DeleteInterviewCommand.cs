using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Questionnaire.Completed
{
    [MapsToAggregateRootMethod(typeof(_CompleteQuestionnaireAR), "DeleteInterview")]
    public class _DeleteInterviewCommand : CommandBase
    {
        public _DeleteInterviewCommand(Guid interviewId, Guid deletedBy)
        {
            this.InterviewId = interviewId;
            this.DeletedBy = deletedBy;
        }

        [AggregateRootId]
        public Guid InterviewId { get; set; }

        public Guid DeletedBy { get; set; }
    }
}
