using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateAnswerInCompleteQuestionnaireHandler : ICommandHandler<UpdateAnswerInCompleteQuestionnaireCommand>
    {
       private ICompleteQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<CompleteQuestionnaire> _conditionExecutor;

        public UpdateAnswerInCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository, IExpressionExecutor<CompleteQuestionnaire> conditionExecutor)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._conditionExecutor = conditionExecutor;
        }

        public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            foreach (CompleteAnswer completeAnswer in command.CompleteAnswers)
            {
                entity.UpdateAnswer(completeAnswer, command.Group);
            }
            RemoveDisabledAnswers(entity);
        }
        protected void RemoveDisabledAnswers(CompleteQuestionnaire entity)
        {
            //innerDocument.CompletedAnswers.RemoveAll(a => a.QuestionPublicKey.Equals(question.PublicKey));
            Questionnaire template = entity.GetQuestionnaireTemplate();
            var questionsToCheck = entity.GetAllAnswers().Select(a => template.Find<Question>(a.QuestionPublicKey)).ToList();
            foreach (var question in questionsToCheck)
            {
                if (!this._conditionExecutor.Execute(entity, question.ConditionExpression))
                    entity.RemoveAnswerOnQuestion(question.PublicKey);
            }
        }
    }
}
