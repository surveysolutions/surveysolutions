using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire
{
    public class MoveQuestionnaireItemHandler : ICommandHandler<MoveQuestionnaireItemCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;

        public MoveQuestionnaireItemHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(MoveQuestionnaireItemCommand command)
        {
            var entity = _questionnaireRepository.Load(command.QuestionnaireId);
            entity.MoveItem(command.PublicKey, command.AfterItemKey);
        }
    }
}
