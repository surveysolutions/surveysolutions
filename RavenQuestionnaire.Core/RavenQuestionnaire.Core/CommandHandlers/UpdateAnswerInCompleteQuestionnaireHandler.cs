using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateAnswerInCompleteQuestionnaireHandler : ICommandHandler<UpdateAnswerInCompleteQuestionnaireCommand>
    {
       private ICompleteQuestionnaireRepository _questionnaireRepository;


       public UpdateAnswerInCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

       public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
       {
           CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
           foreach (CompleteAnswer completeAnswer in command.CompleteAnswers)
           {
               entity.UpdateAnswer(completeAnswer, command.Group);
           }
       }
    }
}
