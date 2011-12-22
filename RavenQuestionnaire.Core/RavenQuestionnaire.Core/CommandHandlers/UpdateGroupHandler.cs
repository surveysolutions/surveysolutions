using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateGroupHandler : ICommandHandler<UpdateGroupCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public UpdateGroupHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(UpdateGroupCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);

            questionnaire.UpdateGroup(command.GroupText, command.GroupPublicKey);
        }
    }
}
