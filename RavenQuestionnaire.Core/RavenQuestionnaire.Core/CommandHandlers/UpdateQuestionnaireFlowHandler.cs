using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateQuestionnaireFlowHandler : ICommandHandler<UpdateQuestionnaireFlowCommand>
    {
         private IQuestionnaireRepository _questionnaireRepository;
         
         public UpdateQuestionnaireFlowHandler(IQuestionnaireRepository questionnaireRepository)
         {
             this._questionnaireRepository = questionnaireRepository;
         }

        public void Handle(UpdateQuestionnaireFlowCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            
            questionnaire.UpdateFlow(command.Blocks, command.Connections);
        }
    }
}
