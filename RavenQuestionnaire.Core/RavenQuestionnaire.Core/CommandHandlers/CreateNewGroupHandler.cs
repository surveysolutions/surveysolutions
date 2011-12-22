using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class CreateNewGroupHandler : ICommandHandler<CreateNewGroupCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public CreateNewGroupHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(CreateNewGroupCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);

            questionnaire.AddGroup(command.GroupText, command.ParentGroupPublicKey);
        }
    }
}
