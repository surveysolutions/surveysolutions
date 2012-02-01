using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateAnswerInCompleteQuestionnaireHandler :
        ICommandHandler<UpdateAnswerInCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<IEnumerable<CompleteAnswer>, bool> _conditionExecutor;
        public UpdateAnswerInCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository,
                                                          IExpressionExecutor<IEnumerable<CompleteAnswer>, bool> conditionExecutor)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._conditionExecutor = conditionExecutor;
        }

        public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            foreach (CompleteAnswer completeAnswer in command.CompleteAnswers)
            {
            //    entity.Remove<CompleteQuestion>(completeAnswer.QuestionPublicKey);
                entity.Add(completeAnswer, null);
            }
            RemoveDisabledAnswers(entity);
        }

        protected void RemoveDisabledAnswers(CompleteQuestionnaire entity)
        {
            //innerDocument.CompletedAnswers.RemoveAll(a => a.QuestionPublicKey.Equals(question.PublicKey));
            //  Questionnaire template = entity.GetQuestionnaireTemplate();
            foreach (CompleteQuestion completeQuestion in entity.GetInnerDocument().Questions)
            {
                if (
                    !this._conditionExecutor.Execute(entity.AnswerIterator, completeQuestion.ConditionExpression))
                {
                    entity.Remove(completeQuestion);
                    completeQuestion.Enabled = false;
                }
                else
                {
                    completeQuestion.Enabled = true;
                }
            }
            var groups = entity.GroupIterator;
            //  var allQuestions = entity.QuestionIterator;
            foreach (CompleteGroup completeGroup in groups)
            {
                if (completeGroup.Propagated && !(completeGroup is IPropogate))
                    continue;

                // IEnumerable<CompleteQuestion> questions = completeGroup is IPropogate ? completeGroup.Questions : allQuestions;
                foreach (CompleteQuestion completeQuestion in completeGroup.Questions)
                {
                    if (
                        !this._conditionExecutor.Execute(
                            completeGroup is PropagatableCompleteGroup
                                ? GetAnswersListForPropagatedGroup(completeGroup as PropagatableCompleteGroup,
                                                                   entity.AnswerIterator)
                                : entity.AnswerIterator,
                            completeQuestion.ConditionExpression))
                    {
                        entity.Remove(completeQuestion);
                        completeQuestion.Enabled = false;
                    }
                    else
                    {
                        completeQuestion.Enabled = true;
                    }
                }
            }
        }

        private IEnumerable<CompleteAnswer> GetAnswersListForPropagatedGroup(PropagatableCompleteGroup group, IEnumerable<CompleteAnswer> allAnswers)
        {
            List<CompleteAnswer> result =
                allAnswers.Where(
                    completeAnswer =>
                    @group.Questions.Count(q => q.PublicKey.Equals(completeAnswer.QuestionPublicKey)) == 0).ToList();
            result.AddRange(group.AnswerIterator);
            return result;
        }
    }
}
