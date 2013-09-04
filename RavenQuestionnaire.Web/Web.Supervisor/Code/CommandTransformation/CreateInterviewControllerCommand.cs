using System;
using System.Collections.Generic;

namespace Web.Supervisor.Code.CommandTransformation
{
    internal class CreateInterviewControllerCommand : IntreviewCommand
    {
        public Guid QuestionnaireId { get; set; }
        public Guid SupervisorId { get; set; }
        public List<UntypedQuestionAnswer> AnswersToFeaturedQuestions { get; set; }
    }
}