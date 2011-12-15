using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeleteQuestionHandler : ICommandHandler<DeleteQuestionCommand>
    {
        
        private IQuestionnaireRepository _questionnaireRepository;
        public DeleteQuestionHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        public void Handle(DeleteQuestionCommand command)
        {
            var entity = _questionnaireRepository.Load(command.QuestionnaireId);
            entity.RemoveQuestion(command.QuestionId);
            //  this._questionRepository.Remove(entity);
            //  entity.
        }
    }
}
