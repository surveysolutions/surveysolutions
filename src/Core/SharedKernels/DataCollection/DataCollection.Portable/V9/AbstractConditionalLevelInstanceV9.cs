using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.V9
{
    public abstract class AbstractConditionalLevelInstanceV9<T> : AbstractConditionalLevelInstanceV8<T>
        where T : IExpressionExecutableV9
    {
        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> GetInstances { get; private set; }

        protected Dictionary<Guid, VariableAccessor> VariableMap { get; private set; }

        protected AbstractConditionalLevelInstanceV9(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.VariableMap = new Dictionary<Guid, VariableAccessor>();
        }

        protected AbstractConditionalLevelInstanceV9(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies,
            IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.Quest = properties;
        }

        private IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV9>> rosterGenerators;

        protected new IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV9>> RosterGenerators
            => this.rosterGenerators ?? (this.rosterGenerators = this.InitializeRosterGenerators());

        private IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV9>> InitializeRosterGenerators()
            => new TwoWayDictionaryAdapter<Guid, Func<decimal[], Identity[], IExpressionExecutableV8>, Func<decimal[], Identity[], IExpressionExecutableV9>>(
                () => base.RosterGenerators, ConvertRosterGeneratorV8ToV9, ConvertRosterGeneratorV9ToV8);

        private static Func<decimal[], Identity[], IExpressionExecutableV9> ConvertRosterGeneratorV8ToV9(Func<decimal[], Identity[], IExpressionExecutableV8> rosterGeneratorV7)
            => (x, y) => (IExpressionExecutableV9)rosterGeneratorV7.Invoke(x, y);

        private static Func<decimal[], Identity[], IExpressionExecutableV8> ConvertRosterGeneratorV9ToV8(Func<decimal[], Identity[], IExpressionExecutableV8> rosterGeneratorV9)
            => rosterGeneratorV9;

        public override IExpressionExecutableV8 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstancesV9)
            => this.CopyMembers(ConvertGetInstancesV8ToV9(getInstancesV9));

        private static Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> ConvertGetInstancesV8ToV9(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstancesV8)
            => (x, y) => getInstancesV8(x, y).Cast<IExpressionExecutableV9>();

        public abstract IExpressionExecutableV9 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> getInstances);

        public new IExpressionExecutableV9 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        protected void AddVariableAccessorToMap(Guid id, VariableAccessor acessor)
        {
            this.VariableMap.Add(id, acessor);
        }

        public void DisableVariable(Guid variableId)
        {
            if (this.EnablementStates.ContainsKey(variableId))
                this.EnablementStates[variableId].State = State.Disabled;
        }

        public void EnableVariable(Guid variableId)
        {
            if (this.EnablementStates.ContainsKey(variableId))
                this.EnablementStates[variableId].State = State.Enabled;
        }
        public VariableValueChanges ProcessVariables()
        {
            var result = new VariableValueChanges();

            foreach (var variableAccessor in VariableMap)
            {
                object newVariableValue = null;
                try
                {
                    newVariableValue = variableAccessor.Value.GetValue();
                }
#pragma warning disable
                catch (Exception ex)
                {
                }
#pragma warning restore
                var previousValue = variableAccessor.Value.GetPreviousValue();
                if (previousValue==null ||!previousValue.Equals(newVariableValue))
                    result.ChangedVariableValues.Add(new Identity(variableAccessor.Key, RosterVector),
                        newVariableValue);
            }

            return result;
        }

        public void SerVariablePreviousValue(Guid variableId, object value)
        {
            if(!VariableMap.ContainsKey(variableId))
                return;
            VariableMap[variableId].SetPreviousValue(value);
        }

        protected new virtual EnablementChanges ProcessEnablementConditionsImpl()
        {
            var enablementChanges = base.ProcessEnablementConditionsImpl();

            var variablesToBeEnabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Variable)
                .Where(StateChangedToEnabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            var variablesToBeDisabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Variable)
                .Where(StateChangedToDisabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            return new EnablementChanges(
                enablementChanges.GroupsToBeDisabled, enablementChanges.GroupsToBeEnabled,
                enablementChanges.QuestionsToBeDisabled, enablementChanges.QuestionsToBeEnabled,
                enablementChanges.StaticTextsToBeDisabled, enablementChanges.StaticTextsToBeEnabled,
                variablesToBeDisabled, variablesToBeEnabled);
        }
    }

    public class VariableAccessor
    {
        public VariableAccessor(Func<object> getValue, Action<object> setPreviousValue, Func<object> getPreviousValue)
        {
            this.GetValue = getValue;
            this.SetPreviousValue = setPreviousValue;
            this.GetPreviousValue = getPreviousValue;
        }

        public Func<object> GetValue { get; private set; }
        public Action<object> SetPreviousValue { get; private set; }
        public Func<object> GetPreviousValue { get; private set; }
    }
}
