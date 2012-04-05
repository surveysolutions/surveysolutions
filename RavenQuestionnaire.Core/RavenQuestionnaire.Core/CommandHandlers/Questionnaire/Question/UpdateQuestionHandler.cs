using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Question
{
    public class UpdateQuestionHandler:ICommandHandler<UpdateQuestionCommand>
    {
         private IQuestionnaireRepository _questionnaireRepository;
         private IExpressionExecutor<Entities.Questionnaire, bool> _expressionValidator;
         public UpdateQuestionHandler(IQuestionnaireRepository questionnaireRepository, IExpressionExecutor<Entities.Questionnaire, bool> validator)
         {
             this._questionnaireRepository = questionnaireRepository;
             this._expressionValidator = validator;
         }

        public void Handle(UpdateQuestionCommand command)
        {
            var questionnaire = _questionnaireRepository.Load(command.QuestionnaireId);
            if (!this._expressionValidator.Execute(questionnaire, command.ConditionExpression))
                return;
            if (!this._expressionValidator.Execute(questionnaire, command.ValidationExpression))
                return;
            questionnaire.UpdateQuestion(command.QuestionPublicKey, command.QuestionText, command.StataExportCaption,
                                        command.QuestionType,
                                         command.ConditionExpression,command.ValidationExpression,
                                         command.Instructions,
                                         command.Answers);

            /*   if(command.Answers!=null)
                questionnaire.UpdateAnswerList(command.Answers);*/
        }
    }
}
