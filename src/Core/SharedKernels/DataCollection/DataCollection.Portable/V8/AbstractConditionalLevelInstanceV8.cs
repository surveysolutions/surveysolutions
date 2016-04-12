using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public abstract class AbstractConditionalLevelInstanceV8<T> : AbstractConditionalLevelInstanceV7<T>, IExpressionExecutableV8
        where T : IExpressionExecutableV8
    {
        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> GetInstances { get; private set; }

        protected AbstractConditionalLevelInstanceV8(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
        }

        protected AbstractConditionalLevelInstanceV8(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies,
            IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.Quest = properties;
        }

        private IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV8>> rosterGenerators;

        protected new IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV8>> RosterGenerators
            => this.rosterGenerators ?? (this.rosterGenerators = this.InitializeRosterGenerators());

        private IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV8>> InitializeRosterGenerators()
            => new TwoWayDictionaryAdapter<Guid, Func<decimal[], Identity[], IExpressionExecutableV7>, Func<decimal[], Identity[], IExpressionExecutableV8>>(
                () => base.RosterGenerators, ConvertRosterGeneratorV7ToV8, ConvertRosterGeneratorV8ToV7);

        private static Func<decimal[], Identity[], IExpressionExecutableV8> ConvertRosterGeneratorV7ToV8(Func<decimal[], Identity[], IExpressionExecutableV7> rosterGeneratorV7)
            => (x, y) => (IExpressionExecutableV8) rosterGeneratorV7.Invoke(x, y);

        private static Func<decimal[], Identity[], IExpressionExecutableV7> ConvertRosterGeneratorV8ToV7(Func<decimal[], Identity[], IExpressionExecutableV7> rosterGeneratorV8)
            => rosterGeneratorV8;

        public override IExpressionExecutableV7 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV7>> getInstancesV7)
            => this.CopyMembers(ConvertGetInstancesV7ToV8(getInstancesV7));

        private static Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> ConvertGetInstancesV7ToV8(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV7>> getInstancesV7)
            => (x, y) => getInstancesV7(x, y).Cast<IExpressionExecutableV8>();

        public abstract IExpressionExecutableV8 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstances);

        public new IExpressionExecutableV8 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        protected abstract void SetParentImpl(IExpressionExecutable parent);
        protected abstract IExpressionExecutableV8 GetParentImpl();

        public void CalculateValidationChanges(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid)
            => this.Validate(out questionsToBeValid, out questionsToBeInvalid);

        public ValidityChanges ProcessValidationExpressions() => this.ExecuteValidations();
        public EnablementChanges ProcessEnablementConditions() => this.ProcessEnablementConditionsImpl();

        public void SetParent(IExpressionExecutable parent) => this.SetParentImpl(parent);
        public void SetParent(IExpressionExecutableV2 parent) => this.SetParentImpl(parent);
        public void SetParent(IExpressionExecutableV5 parent) => this.SetParentImpl(parent);
        public void SetParent(IExpressionExecutableV6 parent) => this.SetParentImpl(parent);
        public void SetParent(IExpressionExecutableV7 parent) => this.SetParentImpl(parent);
        public void SetParent(IExpressionExecutableV8 parent) => this.SetParentImpl(parent);

        IExpressionExecutable IExpressionExecutable.GetParent() => this.GetParentImpl();
        IExpressionExecutableV2 IExpressionExecutableV2.GetParent() => this.GetParentImpl();
        IExpressionExecutableV5 IExpressionExecutableV5.GetParent() => this.GetParentImpl();
        IExpressionExecutableV6 IExpressionExecutableV6.GetParent() => this.GetParentImpl();
        IExpressionExecutableV7 IExpressionExecutableV7.GetParent() => this.GetParentImpl();
        IExpressionExecutableV8 IExpressionExecutableV8.GetParent() => this.GetParentImpl();

        public void DisableStaticText(Guid staticTextId)
        {
            if (this.EnablementStates.ContainsKey(staticTextId))
                this.EnablementStates[staticTextId].State = State.Disabled;
        }

        public void EnableStaticText(Guid staticTextId)
        {
            if (this.EnablementStates.ContainsKey(staticTextId))
                this.EnablementStates[staticTextId].State = State.Enabled;
        }

        protected EnablementChanges ProcessEnablementConditionsImpl()
        {
            foreach (Action enablementCondition in this.ConditionExpressions)
            {
                enablementCondition.Invoke();
            }

            var questionsToBeEnabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Question)
                .Where(StateChangedToEnabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            var questionsToBeDisabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Question)
                .Where(StateChangedToDisabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            var groupsToBeEnabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Group)
                .Where(StateChangedToEnabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            var groupsToBeDisabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Group)
                .Where(StateChangedToDisabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            var staticTextsToBeEnabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.StaticText)
                .Where(StateChangedToEnabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            var staticTextsToBeDisabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.StaticText)
                .Where(StateChangedToDisabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            return new EnablementChanges(
                groupsToBeDisabled, groupsToBeEnabled,
                questionsToBeDisabled, questionsToBeEnabled,
                staticTextsToBeDisabled, staticTextsToBeEnabled);
        }
    }
}