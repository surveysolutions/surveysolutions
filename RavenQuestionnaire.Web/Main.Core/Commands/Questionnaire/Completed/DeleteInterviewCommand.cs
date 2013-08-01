using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Questionnaire.Completed
{
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "DeleteInterview")]
    public class DeleteInterviewCommand : CommandBase
    {
        [AggregateRootId]
        public Guid InterviewId { get; set; }

        public Guid DeletedBy { get; set; }

        public DeleteInterviewCommand(Guid interviewId, Guid deletedBy)
        {
            this.InterviewId = interviewId;
            this.DeletedBy = deletedBy;
        }
    }
}
