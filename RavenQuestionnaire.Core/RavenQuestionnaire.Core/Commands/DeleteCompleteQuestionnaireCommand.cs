using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class DeleteCompleteQuestionnaireCommand : ICommand
    {
        public string CompleteQuestionnaireId { get; set; }

        public DeleteCompleteQuestionnaireCommand(string completeQeuestionnaireId)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQeuestionnaireId);
        }
    }
}
