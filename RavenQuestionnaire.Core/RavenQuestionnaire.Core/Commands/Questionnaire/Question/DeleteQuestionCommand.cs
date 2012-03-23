using System;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    public class DeleteQuestionCommand : ICommand
    {
        public Guid QuestionId { get; set; }
        public string QuestionnaireId { get; set; }

        public UserLight Executor { get; set; }

        public DeleteQuestionCommand(Guid questionId, string questionnaireId, UserLight executor)
        {
            this.QuestionId = questionId;
            Executor = executor;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }
    }
}
