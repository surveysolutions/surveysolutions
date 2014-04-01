using System;
using System.Collections.Generic;
using Ncqrs.Commanding;

namespace WB.UI.Headquarters.Api.Models
{
    internal class CreateInterviewControllerCommand : ICommand
    {
        public Guid QuestionnaireId { get; set; }
        public Guid SupervisorId { get; set; }
        public Guid InterviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid CommandIdentifier { get; set; }
        public long? KnownVersion { get; set; }
        public List<UntypedQuestionAnswer> AnswersToFeaturedQuestions { get; set; }
    }
}