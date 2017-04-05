using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public Guid SupervisorId { get; private set; }
        public Dictionary<Guid, AbstractAnswer> AnswersToFeaturedQuestions { get; private set; }
        public DateTime AnswersTime { get; private set; }

        public InterviewKey InterviewKey { get; private set; }

        public CreateInterviewCommand(Guid interviewId,
            Guid userId,
            Guid questionnaireId,
            Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions,
            DateTime answersTime,
            Guid supervisorId,
            long questionnaireVersion, 
            InterviewKey interviewKey)
            : base(interviewId, userId)
        {
            this.QuestionnaireVersion = questionnaireVersion;
            this.InterviewKey = interviewKey;
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.AnswersToFeaturedQuestions = answersToFeaturedQuestions;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
        }
    }
}
