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
    public class AddNewQuestionHandler : ICommandHandler<AddNewQuestionCommand>
    {
        private IQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<Questionnaire> _expressionValidator;
        public AddNewQuestionHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Questionnaire> validator)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._expressionValidator = validator;
            //    this._questionUploader = questionUploader;
        }

        public void Handle(AddNewQuestionCommand command)
        {
            var questionnaire = this._questionnaireRepository.Load(command.QuestionnaireId);
            if (!this._expressionValidator.Execute(questionnaire, command.ConditionExpression))
                return;
            var question = questionnaire.AddQuestion(command.QuestionText, command.StataExportCaption,
                                                     command.QuestionType,
                                                     command.ConditionExpression, 
                                                     command.GroupPublicKey);
            question.UpdateAnswerList(command.Answers);
        }
    }
}
