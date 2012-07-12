using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateQuestionnaireHandler:ICommandHandler<UpdateQuestionnaireCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;

        public UpdateQuestionnaireHandler(IQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }
        public void Handle(UpdateQuestionnaireCommand command)
        {
            var entity = this._questionnaireRepository.Load(command.QuestionnaireId);
            entity.UpdateText(command.Title);
        }
    }
}
