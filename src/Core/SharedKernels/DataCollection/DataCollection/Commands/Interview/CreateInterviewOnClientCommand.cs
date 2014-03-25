using System;
using System.Collections.Generic;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootConstructor(typeof(Implementation.Aggregates.Interview))]
    public class CreateInterviewOnClientCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public Guid SupervisorId { get; private set; }
        //public Dictionary<Guid, object> AnswersToFeaturedQuestions { get; private set; }
        public DateTime AnswersTime { get; private set; }

        //public string DeviceId { get; private set; }

        public CreateInterviewOnClientCommand(Guid interviewId, Guid userId, Guid questionnaireId, 
             DateTime answersTime, Guid supervisorId)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            
            this.AnswersTime = answersTime;

            this.SupervisorId = supervisorId;
            
        }
    }
}
