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
    public class UpdateConditionsHandler : ICommandHandler<UpdateConditionsCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<Questionnaire, bool> _expressionValidator;
        public UpdateConditionsHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Questionnaire, bool> validator)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._expressionValidator = validator;
        }

        public void Handle(UpdateConditionsCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            foreach (var condition in command.Conditions)
            {
                if (!this._expressionValidator.Execute(questionnaire, condition.Value))
                    continue;
                questionnaire.UpdateConditionExpression(condition.Key, condition.Value);
            }
        }
    }
}
