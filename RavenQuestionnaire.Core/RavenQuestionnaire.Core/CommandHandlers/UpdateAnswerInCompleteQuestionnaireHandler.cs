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
        private IExpressionExecutor<IEnumerable<ICompleteAnswer>, bool> _conditionExecutor;
        public UpdateAnswerInCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository,
                                                          IExpressionExecutor<IEnumerable<ICompleteAnswer>, bool> conditionExecutor)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._conditionExecutor = conditionExecutor;
        }

        public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);

            //    entity.Remove<CompleteQuestion>(completeAnswer.QuestionPublicKey);
            foreach (var completeAnswer in command.CompleteAnswers)
            {
                if (completeAnswer.Selected)
                    entity.Add(completeAnswer, null);
                else
                    entity.Remove(completeAnswer);
            }
            RemoveDisabledAnswers(entity);
        }

        protected void RemoveDisabledAnswers(CompleteQuestionnaire entity)
        {
            //innerDocument.CompletedAnswers.RemoveAll(a => a.QuestionPublicKey.Equals(question.PublicKey));
            //  Questionnaire template = entity.GetQuestionnaireTemplate();
            foreach (ICompleteQuestion completeQuestion in entity.GetInnerDocument().Questions)
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
                foreach (ICompleteQuestion completeQuestion in completeGroup.Questions)
                {
                    if (
                        !this._conditionExecutor.Execute(
                            completeGroup is PropagatableCompleteGroup
                                ? GetAnswersListForPropagatedGroup(completeGroup as PropagatableCompleteGroup,
                                                                   entity)
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
        private IEnumerable<ICompleteAnswer> GetAnswersListForPropagatedGroup(PropagatableCompleteGroup group, CompleteQuestionnaire questionnaire)
        {
            List<ICompleteAnswer> result = new List<ICompleteAnswer>();
            var propagatedGroupWithSameId =
                questionnaire.Find<PropagatableCompleteGroup>(
                    g => g.PropogationPublicKey.Equals(group.PropogationPublicKey));

            if (questionnaire.AnswerIterator != null)
                result =
                    questionnaire.AnswerIterator.Where(
                        a => propagatedGroupWithSameId.SelectMany(g=>g.Questions).Count(ag => ag.PublicKey.Equals(a.QuestionPublicKey)) == 0).ToList();
            result.AddRange(propagatedGroupWithSameId.SelectMany(g => g.AnswerIterator));
            return result;
        }

        private IEnumerable<ICompleteAnswer> GetAnswersListForPropagatedGroup(PropagatableCompleteGroup group, IEnumerable<ICompleteAnswer> allAnswers)
        {
            List<ICompleteAnswer> result = new List<ICompleteAnswer>();
            if (allAnswers != null)
                result = allAnswers.Where(
                    completeAnswer =>
                    group.Questions.Count(q => q.PublicKey.Equals(completeAnswer.QuestionPublicKey)) == 0).ToList();
            result.AddRange(group.AnswerIterator);
            return result;
        }
    }
}
