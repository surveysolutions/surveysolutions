using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewOnClientCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; private set; }
        public Guid SupervisorId { get; private set; }
        public InterviewKey InterviewKey { get; }
        public int? AssignmentId { get; private set; }
        public Dictionary<Guid, AbstractAnswer> AnswersToIdentifyingQuestions { get; private set; }
        public DateTime AnswersTime { get; private set; }

        public CreateInterviewOnClientCommand(Guid interviewId, 
            Guid userId, 
            QuestionnaireIdentity questionnaireIdentity, 
            DateTime answersTime, Guid supervisorId, 
            InterviewKey interviewKey, 
            int? assignmentId,
            Dictionary<Guid, AbstractAnswer> answersToIdentifyingQuestions)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireIdentity = questionnaireIdentity;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
            this.InterviewKey = interviewKey;
            this.AssignmentId = assignmentId;
            this.AnswersToIdentifyingQuestions = answersToIdentifyingQuestions ?? new Dictionary<Guid, AbstractAnswer>();
        }
    }
}
