using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeleteQuestionnaireHandler: ICommandHandler<DeleteQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        public DeleteQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(DeleteQuestionnaireCommand command)
        {
            var entity = _questionnaireRepository.Load(command.QuestionnaireId);
            this._questionnaireRepository.Remove(entity);
        }
    }
}
