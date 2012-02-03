using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class PropagateGroupHandler : ICommandHandler<PropagateGroupCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;
        private IExpressionExecutor<IEnumerable<CompleteAnswer>, bool> _conditionExecutor;

        public PropagateGroupHandler(ICompleteQuestionnaireRepository questionnaireRepository,
                                                          IExpressionExecutor<IEnumerable<CompleteAnswer>, bool> conditionExecutor)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._conditionExecutor = conditionExecutor;
        }

        #region Implementation of ICommandHandler<PropagateGroupCommand>

        public void Handle(PropagateGroupCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            var template = entity.Find<CompleteGroup>(command.GroupPublicKey);
            bool isCondition = false;

            foreach (CompleteQuestion completeQuestion in template.Questions)
            {
                if (
                    this._conditionExecutor.Execute(entity.AnswerIterator,
                                                    completeQuestion.ConditionExpression))
                {
                    isCondition = true;
                    completeQuestion.Enabled = true;
                }
                else
                {
                    completeQuestion.Enabled = false;
                }
            }
            if (isCondition)
            {
                entity.Add(entity.Find<CompleteGroup>(command.GroupPublicKey), null);
                return;
            }
            throw new InvalidOperationException("Group can't be added");

        }

        #endregion
    }
}
