using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation
{
    internal class CreateInterviewControllerCommand : IntreviewCommand
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid SupervisorId { get; set; }
        public List<UntypedQuestionAnswer> AnswersToFeaturedQuestions { get; set; }
    }
}