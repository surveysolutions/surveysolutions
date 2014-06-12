using System;
using System.Collections.Generic;

namespace WB.UI.Headquarters.Code.CommandTransformation
{
    internal class CreateInterviewControllerCommand : IntreviewCommand
    {
        public Guid QuestionnaireId { get; set; }
        public Guid SupervisorId { get; set; }
        public List<UntypedQuestionAnswer> AnswersToFeaturedQuestions { get; set; }
    }
}