using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.V9
{
    public abstract class AbstractInterviewExpressionStateV9 : AbstractInterviewExpressionStateV8, IInterviewExpressionStateV9
    {
        protected AbstractInterviewExpressionStateV9() {}

        protected AbstractInterviewExpressionStateV9(IDictionary<string, IExpressionExecutableV9> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : this(interviewScopes.AsEnumerable(), siblingRosters, interviewProperties) {}

        protected AbstractInterviewExpressionStateV9(IEnumerable<KeyValuePair<string, IExpressionExecutableV9>> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
            : base(
                interviewScopes.ToDictionary<KeyValuePair<string, IExpressionExecutableV9>, string, IExpressionExecutableV8>(
                    item => item.Key,
                    item => item.Value),
                siblingRosters,
                interviewProperties) {}

        private IDictionary<string, IExpressionExecutableV9> interviewScopes;

        public new IDictionary<string, IExpressionExecutableV9> InterviewScopes
            => this.interviewScopes ?? (this.interviewScopes = this.InitializeInterviewScopes());

        private IDictionary<string, IExpressionExecutableV9> InitializeInterviewScopes()
            => new TwoWayDictionaryAdapter<string, IExpressionExecutableV8, IExpressionExecutableV9>(
                () => base.InterviewScopes, ConvertExpressionExecutableV8ToV9, ConvertExpressionExecutableV9ToV8);

        private static IExpressionExecutableV8 ConvertExpressionExecutableV9ToV8(IExpressionExecutableV9 expressionExecutableV9)
            => expressionExecutableV9;

        private static IExpressionExecutableV9 ConvertExpressionExecutableV8ToV9(IExpressionExecutableV8 expressionExecutableV8)
        {
            if (expressionExecutableV8 == null)
                return null;

            if (expressionExecutableV8 is IExpressionExecutableV9)
                return (IExpressionExecutableV9) expressionExecutableV8;

            throw new NotSupportedException($"Interview scope expression executable V8 ({expressionExecutableV8.GetType().FullName}) cannot be converted to V9");
        }

        public new IEnumerable<IExpressionExecutableV9> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
            => base.GetRosterInstances(rosterKey, scopeId).Cast<IExpressionExecutableV9>();

        public new virtual EnablementChanges ProcessEnablementConditions()
            => EnablementChanges.Union(
                this.InterviewScopes
                    .Values
                    .OrderBy(x => x.GetLevel()) // order by scope depth starting from top because conditionally lower scope could depend only from upper scope
                    .Select(scope => scope.ProcessEnablementConditions()));

        protected new virtual IExpressionExecutableV9 GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
            => (IExpressionExecutableV9) base.GetRosterByIdAndVector(questionId, rosterVector);

        public VariableValueChanges ProcessVariables()
        {
            var result = new VariableValueChanges();

            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                result.AppendChanges(interviewScopeKvpValue.ProcessVariables());
            }
            return result;
        }

        public void DisableVariables(IEnumerable<Identity> variablesToDisable)
        {
            foreach (Identity variable in variablesToDisable)
            {
                var targetLevel = this.GetRosterByIdAndVector(variable.Id, variable.RosterVector);
                if (targetLevel == null) return;

                targetLevel.DisableVariable(variable.Id);
            }
        }

        public void EnableVariables(IEnumerable<Identity> variablesToEnable)
        {
            foreach (Identity variable in variablesToEnable)
            {
                var targetLevel = this.GetRosterByIdAndVector(variable.Id, variable.RosterVector);
                if (targetLevel == null) return;

                targetLevel.EnableVariable(variable.Id);
            }
        }

        public void SerVariablePreviousValue(Identity variableId, object value)
        {
            var targetLevel = this.GetRosterByIdAndVector(variableId.Id, variableId.RosterVector);
            if (targetLevel == null) return;

            targetLevel.SerVariablePreviousValue(variableId.Id, value);
        }

        IInterviewExpressionStateV9 IInterviewExpressionStateV9.Clone() => (IInterviewExpressionStateV9) this.Clone();
    }
}