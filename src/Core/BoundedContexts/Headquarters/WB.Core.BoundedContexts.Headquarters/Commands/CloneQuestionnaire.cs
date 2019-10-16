using System;

namespace WB.Core.BoundedContexts.Headquarters.Commands
{
    public class CloneQuestionnaire : QuestionnaireCommand
    {
        public CloneQuestionnaire(Guid questionnaireId, long sourceQuestionnaireVersion, string newTitle,
            long newQuestionnaireVersion, Guid userId, string comment)
            : base(Guid.NewGuid(), questionnaireId)
        {
            this.SourceQuestionnaireVersion = sourceQuestionnaireVersion;
            this.NewTitle = newTitle;
            this.NewQuestionnaireVersion = newQuestionnaireVersion;
            this.UserId = userId;
            this.Comment = comment;
        }

        public string Comment { get; }

        public long SourceQuestionnaireVersion { get; }
        public string NewTitle { get; }
        public long NewQuestionnaireVersion { get; set; }
        public Guid UserId { get; }
    }
}
