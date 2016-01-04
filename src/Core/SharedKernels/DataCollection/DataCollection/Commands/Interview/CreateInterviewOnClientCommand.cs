using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewOnClientCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public Guid SupervisorId { get; private set; }
        //public Dictionary<Guid, object> AnswersToFeaturedQuestions { get; private set; }
        public DateTime AnswersTime { get; private set; }

        //public string DeviceId { get; private set; }

        public CreateInterviewOnClientCommand(Guid interviewId, Guid userId, Guid questionnaireId, long questionnaireVersion,
             DateTime answersTime, Guid supervisorId)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            
            this.AnswersTime = answersTime;

            this.SupervisorId = supervisorId;
            this.QuestionnaireVersion = questionnaireVersion;
        }
    }
}
