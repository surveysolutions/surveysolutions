using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class DeleteQuestionnaireCommand : ICommand
    {
         public string QuestionnaireId { get; set; }

         public DeleteQuestionnaireCommand(string questionnaireId)
         {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
        }
    }
}
