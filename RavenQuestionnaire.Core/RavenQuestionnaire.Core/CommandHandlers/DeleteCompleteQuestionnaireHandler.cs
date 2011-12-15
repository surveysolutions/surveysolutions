using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeleteCompleteQuestionnaireHandler:ICommandHandler<DeleteCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireRepository _repository;
        public DeleteCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository repository)
        {
            this._repository = repository;
        }

        public void Handle(DeleteCompleteQuestionnaireCommand command)
        {
            var entity = _repository.Load(command.CompleteQuestionnaireId);
            this._repository.Remove(entity);
        }
    }
}
