using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class DeleteQuestionCommand : ICommand
    {
        public Guid QuestionId { get; set; }
        public string QuestionnaireId { get; set; }

        public DeleteQuestionCommand(Guid questionId, string questionnaireId)
        {
            this.QuestionId = questionId;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }
    }
}
