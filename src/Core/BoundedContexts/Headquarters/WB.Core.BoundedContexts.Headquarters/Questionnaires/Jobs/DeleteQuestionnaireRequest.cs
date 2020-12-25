using System;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
    public class DeleteQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public long Version { get; set; }
        public Guid UserId { get; set; }
    }
}