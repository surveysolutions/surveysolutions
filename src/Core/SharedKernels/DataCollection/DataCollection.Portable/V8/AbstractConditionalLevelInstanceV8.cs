using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public abstract class AbstractConditionalLevelInstanceV8<T> : AbstractConditionalLevelInstanceV7<T>
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

        protected EnablementChanges ProcessEnablementConditionsImpl()
        {
            List<Identity> questionsToBeEnabled;
            List<Identity> questionsToBeDisabled;
            List<Identity> groupsToBeEnabled;
            List<Identity> groupsToBeDisabled;

            this.CalculateConditionChanges(
                out questionsToBeEnabled,
                out questionsToBeDisabled,
                out groupsToBeEnabled,
                out groupsToBeDisabled);

            return new EnablementChanges(groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled);
        }
    }
}