using NCalc;
using System;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public class CompleteQuestionnaireConditionExecutor
    {
        private readonly GroupHash hash;

        public CompleteQuestionnaireConditionExecutor(GroupHash hash)
        {
            this.hash = hash;
        }

        public bool Execute(ICompleteQuestion question)
        {
            if (string.IsNullOrEmpty(question.ConditionExpression))
                return true;
            var e = new Expression(question.ConditionExpression);
            e.EvaluateParameter += (name, args) =>
            {
                Guid nameGuid = Guid.Parse(name);
                Guid? propagationKey = question.PropogationPublicKey;
                var value = hash[nameGuid, propagationKey].GetAnswerObject();
                args.Result = value;
            };
            bool result = false;
            try
            {
                result = (bool)e.Evaluate();
            }
            catch (Exception)
            {
            }
            return result;
        }

        public bool Execute(ICompleteGroup group)
        {
            bool result = ExecuteGroup(group);
            UpdateAllChildElementsInGroup(group, result);
            return result;
        }

        private bool ExecuteGroup(ICompleteGroup group)
        {
            if (string.IsNullOrEmpty(group.ConditionExpression))
                return true;
            var e = new Expression(group.ConditionExpression);
            e.EvaluateParameter += (name, args) =>
            {
                Guid nameGuid = Guid.Parse(name);
                Guid? propagationKey = group.PropogationPublicKey;
                var value = hash[nameGuid, propagationKey].GetAnswerObject();
                args.Result = value ?? string.Empty;

            };
            bool result = false;
            try
            {
                result = (bool)e.Evaluate();
            }
            catch (Exception)
            {
            }
            return result;
        }

        private void UpdateAllChildElementsInGroup(ICompleteGroup group, bool result)
        {
            foreach (IComposite child in group.Children)
            {
                var question = child as ICompleteQuestion;
                if (question != null)
                {
                    question.Enabled = result && Execute(question);
                    continue;
                }

                var gr = child as ICompleteGroup;
                if (gr != null)
                    gr.Enabled = result && Execute(gr);

            }
        }
    }
}
