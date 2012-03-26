using System;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class DeleteImageCommand : ICommand
    {
        public DeleteImageCommand(string questionnaireId, Guid questionKey, Guid imageKey, UserLight executor)
        {
            QuestionKey = questionKey;
            ImageKey = imageKey;
            QuestionnaireId = questionnaireId;
            Executor = executor;
        }

        public Guid QuestionKey { get; set; }

        public Guid ImageKey { get; set; }

        public string QuestionnaireId { get; set; }

        #region ICommand Members

        public UserLight Executor { get; set; }

        #endregion
    }
}