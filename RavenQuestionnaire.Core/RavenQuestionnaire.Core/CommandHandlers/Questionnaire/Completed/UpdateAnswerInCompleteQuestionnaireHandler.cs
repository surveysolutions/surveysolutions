using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
{
    public class UpdateAnswerInCompleteQuestionnaireHandler :
        ICommandHandler<UpdateAnswerInCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;
    //    private IExpressionExecutor<IEnumerable<ICompleteAnswer>, bool> _conditionExecutor;
        public UpdateAnswerInCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository/*,
                                                          IExpressionExecutor<IEnumerable<ICompleteAnswer>, bool> conditionExecutor*/)
        {
            this._questionnaireRepository = questionnaireRepository;
       //     this._conditionExecutor = conditionExecutor;
        }

        public void Handle(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            foreach (var completeAnswer in command.CompleteAnswers)
            {
                if (completeAnswer.Selected)
                    entity.Add(completeAnswer, null);
                else
                    entity.Remove(completeAnswer);
            }
            PropagatableCompleteAnswer propagated = command.CompleteAnswers[0] as PropagatableCompleteAnswer;
            ICompleteGroup general = entity.GetInnerDocument();
            if (propagated == null)
                ExecuteConditions(FindQuestion(command.CompleteAnswers[0].QuestionPublicKey, null, general), general);
            else

                ExecuteConditions(FindQuestion(propagated.QuestionPublicKey, propagated.PropogationPublicKey, general),
                                  general);
            /*   var questions = entity.GetInnerDocument().GetAllQuestions();
            var executor = new CompleteQuestionnaireConditionExecutor(entity);
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                completeQuestion.Enabled = executor.Execute(completeQuestion);
                if (!completeQuestion.Enabled)
                    entity.Remove(completeQuestion);
            }*/
        }

        protected void ExecuteConditions(ICompleteQuestion question, ICompleteGroup entity)
        {
            PropagatableCompleteQuestion propagated = question as PropagatableCompleteQuestion;
            IEnumerable<ICompleteQuestion> triggeres;
            if(propagated==null)
            {
                triggeres =
               entity.Find<ICompleteQuestion>(
                   g => g.Triggers.Count(gp => gp.Equals(question.PublicKey)) > 0).ToList();
            }
            else
            {
                triggeres =
                    entity.GetPropagatedGroupsByKey(propagated.PropogationPublicKey).SelectMany(g => g.Find<ICompleteQuestion>(
                        q => q.Triggers.Count(gp => gp.Equals(question.PublicKey)) > 0)).ToList();
            }
            var executor = new CompleteQuestionnaireConditionExecutor(entity);
            foreach (ICompleteQuestion completeQuestion in triggeres)
            {
                bool previousState = completeQuestion.Enabled;
                completeQuestion.Enabled = executor.Execute(completeQuestion);
                if (!completeQuestion.Enabled)
                    entity.Remove(completeQuestion);
                if(previousState!=completeQuestion.Enabled)
                {
                    ExecuteConditions(completeQuestion, entity);
                }
            }
        }
        protected ICompleteQuestion FindQuestion(Guid questionKey, Guid? propagationKey, ICompleteGroup entity)
        {
            //PropagatableCompleteAnswer propagated = answer as PropagatableCompleteAnswer;
            if (!propagationKey.HasValue)
                return entity.FirstOrDefault<ICompleteQuestion>(q => q.PublicKey == questionKey);
            return entity.GetPropagatedQuestion(questionKey, propagationKey.Value);
        }

        /* protected void RemoveDisabledAnswers(CompleteQuestionnaire entity)
        {
            //innerDocument.CompletedAnswers.RemoveAll(a => a.QuestionPublicKey.Equals(question.PublicKey));
            //  Questionnaire template = entity.GetQuestionnaireTemplate();
            foreach (ICompleteQuestion completeQuestion in entity.GetInnerDocument().Questions)
            {
                if (
                    !this._conditionExecutor.Execute(entity.Find<ICompleteAnswer>(a => a.Selected), completeQuestion.ConditionExpression))
                {
                    entity.Remove(completeQuestion);
                    completeQuestion.Enabled = false;
                }
                else
                {
                    completeQuestion.Enabled = true;
                }
            }
            var groups = entity.Find<ICompleteGroup>(q => true);
            //  var allQuestions = entity.QuestionIterator;
            foreach (CompleteGroup completeGroup in groups)
            {
                if (completeGroup.Propagated != Propagate.None && !(completeGroup is IPropogate))
                    continue;

                // IEnumerable<CompleteQuestion> questions = completeGroup is IPropogate ? completeGroup.Questions : allQuestions;
                foreach (ICompleteQuestion completeQuestion in completeGroup.Questions)
                {
                    if (
                        !this._conditionExecutor.Execute(
                            completeGroup is PropagatableCompleteGroup
                                ? GetAnswersListForPropagatedGroup(completeGroup as PropagatableCompleteGroup,
                                                                   entity)
                                : entity.Find<ICompleteAnswer>(a => a.Selected),
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
            result =
                questionnaire.Find<ICompleteAnswer>(a => a.Selected).Where(
                    a =>
                    propagatedGroupWithSameId.SelectMany(g => g.Questions).Count(
                        ag => ag.PublicKey.Equals(a.QuestionPublicKey)) == 0).ToList();
            result.AddRange(propagatedGroupWithSameId.SelectMany(g => g.Find<ICompleteAnswer>(a => a.Selected)));
            return result;
        }*/
    }
}
