using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class DeleteGroupCommand : ICommand
    {
        public Guid GroupPublicKey { get; set; }
        public string QuestionnaireId { get; set; }

        public DeleteGroupCommand(Guid groupPublicKey, string questionnaireId)
        {
            this.GroupPublicKey = groupPublicKey;
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }
    }
}
