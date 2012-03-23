using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    public class DeleteGroupCommand : ICommand
    {
        public Guid GroupPublicKey { get; set; }
        public string QuestionnaireId { get; set; }
        public UserLight Executor { get; set; }

        public DeleteGroupCommand(Guid groupPublicKey, string questionnaireId, UserLight executor)
        {
            this.GroupPublicKey = groupPublicKey;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            Executor = executor;
        }
    }
}
