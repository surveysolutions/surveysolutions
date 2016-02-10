using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewDetailsApiView
    {
        public List<InterviewAnswerOnPrefilledQuestionApiView> AnswersOnPrefilledQuestions { get; set; }
        public string LastSupervisorOrInterviewerComment { get; set; }
        public DateTime? RejectedDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }

        public List<InterviewAnswerApiView> Answers { get; set; }

        public List<IdentityApiView> DisabledGroups { get; set; }
        public List<IdentityApiView> DisabledQuestions { get; set; }
        public List<IdentityApiView> ValidAnsweredQuestions { get; set; }
        public List<IdentityApiView> InvalidAnsweredQuestions { get; set; }

        public Dictionary<IdentityApiView, List<FailedValidationCondition>> FailedValidationConditions { get; set; }

        public List<RosterApiView> RosterGroupInstances { get; set; }

        public bool WasCompleted { get; set; }
    }

    public class InterviewAnswerApiView
    {
        public Guid QuestionId { get; set; }

        public decimal[] QuestionRosterVector { get; set; }

        public string JsonAnswer { get; set; }
        public string LastSupervisorOrInterviewerComment { get; set; }
    }
}