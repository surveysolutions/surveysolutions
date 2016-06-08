using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.V10
{
    public abstract class AbstractConditionalLevelInstanceV10<T> : AbstractConditionalLevelInstanceV9<T>
        where T : IExpressionExecutableV10
    {
        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> GetInstances { get; private set; }

        protected Dictionary<Guid, Func<long, string, long?, bool>> OptionFiltersMap { get; } = new Dictionary<Guid, Func<long, string, long?, bool>>();

        protected Dictionary<Guid, Guid[]> DependentAnswersToVerify { get; set; }

        protected AbstractConditionalLevelInstanceV10(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies, Dictionary<Guid, Guid[]> dependentAnswersToVerify)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.DependentAnswersToVerify = dependentAnswersToVerify;
        }

        protected AbstractConditionalLevelInstanceV10(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies,
            IInterviewProperties properties, Dictionary<Guid, Guid[]> dependentAnswersToVerify)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies, dependentAnswersToVerify)
        {
            this.Quest = properties;
        }

        protected abstract Guid[] GetRosterScopeIds(Guid rosterId);

        protected abstract Guid GetQuestionnaireId();

        private IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV10>> rosterGenerators;

        protected new virtual IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV10>> RosterGenerators
            => this.rosterGenerators ?? (this.rosterGenerators = this.InitializeRosterGenerators());

        private IDictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV10>> InitializeRosterGenerators()
            => new TwoWayDictionaryAdapter<Guid, Func<decimal[], Identity[], IExpressionExecutableV9>, Func<decimal[], Identity[], IExpressionExecutableV10>>(
                () => base.RosterGenerators, ConvertRosterGeneratorV9ToV10, ConvertRosterGeneratorV10ToV9);

        private static Func<decimal[], Identity[], IExpressionExecutableV10> ConvertRosterGeneratorV9ToV10(Func<decimal[], Identity[], IExpressionExecutableV9> rosterGeneratorV7)
            => (x, y) => (IExpressionExecutableV10)rosterGeneratorV7.Invoke(x, y);

        private static Func<decimal[], Identity[], IExpressionExecutableV9> ConvertRosterGeneratorV10ToV9(Func<decimal[], Identity[], IExpressionExecutableV9> rosterGeneratorV10)
            => rosterGeneratorV10;

        public override IExpressionExecutableV9 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> getInstancesV10)
            => this.CopyMembers(ConvertGetInstancesV9ToV10(getInstancesV10));

        private static Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> ConvertGetInstancesV9ToV10(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> getInstancesV9)
            => (x, y) => getInstancesV9(x, y)?.Cast<IExpressionExecutableV10>();

        public abstract IExpressionExecutableV10 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances);

        public new virtual IExpressionExecutableV10 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        private State GetConditionExpressionState(Func<bool> expression)
        {
            try
            {
                return expression() ? State.Enabled : State.Disabled;
            }
            catch
            {
                return State.Disabled;
            }
        }

        protected virtual Action AnswerVerifier(Func<long, string, long?, bool> optionFilter, Guid itemId, Func<decimal?> getAnswer, Action<decimal?> setAnswer)
        {
            return () =>
            {
                if (getAnswer().HasValue && optionFilter(Convert.ToInt64(getAnswer().Value), "", null) == false)
                {
                    setAnswer(null);
                }
            };
        }

        protected virtual Action AnswerVerifier(Func<long, string, long?, bool> optionFilter, Guid itemId, Func<decimal[]> getAnswer, Action<decimal[]> setAnswer)
        {
            return () =>
            {
                decimal[] previousAnswer = getAnswer();
                if (previousAnswer == null || previousAnswer.Length == 0)
                    return;

                var actualAnswer = previousAnswer.Where(selectedOption => optionFilter(Convert.ToInt64(selectedOption), "", null));
                setAnswer(actualAnswer.ToArray());
            };
        }

        protected new Action Verifier(Func<bool> isEnabled, Guid itemId, ConditionalState questionState)
        {
            return () =>
            {
                if (questionState.State == State.Disabled)
                    return;

                questionState.State = this.GetConditionExpressionState(isEnabled);

                var hasNoDependentAnswersToBeVerified = !DependentAnswersToVerify.ContainsKey(itemId) || !DependentAnswersToVerify[itemId].Any();
                if (!hasNoDependentAnswersToBeVerified)
                {
                    foreach (var questionId in DependentAnswersToVerify[itemId])
                    {
                        VerifyAnswer(questionId);
                    }
                }

                this.UpdateAllNestedItemsStateAndPropagateStateOnRosters(itemId, this.StructuralDependencies, questionState.State);
            };
        }

        private void VerifyAnswer(Guid questionId)
        {
            this.QuestionDecimalUpdateMap[questionId].Invoke(null);
        }

        protected void UpdateAllNestedItemsStateAndPropagateStateOnRosters(Guid itemId, Dictionary<Guid, Guid[]> structureDependencies, State state)
        {
            var hasNoStructureDependencies = !structureDependencies.ContainsKey(itemId) || !structureDependencies[itemId].Any();
            
            if (hasNoStructureDependencies) return;

            var stack = new Queue<Guid>(structureDependencies[itemId]);
            while (stack.Any())
            {
                var id = stack.Dequeue();

                if (this.EnablementStates.ContainsKey(id))
                {
                    this.EnablementStates[id].State = state;
                }
                else
                {
                    var rosterScope = GetRosterScopeIds(id);

                    var isQuestionnaireLevel = this.RosterKey.Length == 1 && this.RosterKey[0].Id == this.GetQuestionnaireId();

                    var rosterKey = isQuestionnaireLevel
                        ? new Identity[0]
                        : this.RosterKey;

                    var rosters = this.GetInstances(rosterKey, rosterScope.Last());
                    if (rosters != null)
                    {
                        foreach (var roster in rosters)
                        {
                            if (state == State.Disabled)
                            {
                                roster.DisableGroup(id);
                            }
                            if (state == State.Enabled)
                            {
                                roster.EnableGroup(id);
                            }
                        }
                    }
                }

                if (structureDependencies.ContainsKey(id) && structureDependencies[id].Any())
                {
                    foreach (var dependentQuestionId in structureDependencies[id])
                    {
                        stack.Enqueue(dependentQuestionId);
                    }
                }
            }
        }

        public IEnumerable<CategoricalOption> FilterOptionsForQuestion(Guid questionId, IEnumerable<CategoricalOption> options)
        {
            if (!OptionFiltersMap.ContainsKey(questionId))
            {
                foreach (var option in options)
                    yield return option;
            }

            var filter = OptionFiltersMap[questionId];
            foreach (var option in options)
            {
                var isOptionSatisfyFilter = filter(option.Value, option.Title, option.ParentValue);
                if (isOptionSatisfyFilter)
                {
                    yield return option;
                }
            }
        }
    }
}