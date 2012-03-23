using System;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Group
{
    public class PropagateGroupHandler : ICommandHandler<PropagateGroupCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;
      

        public PropagateGroupHandler(ICompleteQuestionnaireRepository questionnaireRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
        }

        #region Implementation of ICommandHandler<PropagateGroupCommand>

        public void Handle(PropagateGroupCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            var template = entity.Find<CompleteGroup>(command.GroupPublicKey);
            bool isCondition = false;
            var executor = new CompleteQuestionnaireConditionExecutor(entity.GetInnerDocument());
            foreach (CompleteQuestion completeQuestion in template.Questions)
            {
                if (
                    executor.Execute(
                                                                         completeQuestion))
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
