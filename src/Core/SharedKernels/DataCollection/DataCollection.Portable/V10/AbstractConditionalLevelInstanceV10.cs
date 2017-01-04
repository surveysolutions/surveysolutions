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

        public Action<Identity[], Guid, decimal> RemoveRosterInstances { get; private set; }
        public StructuralChanges StructuralChangesCollector { get; private set; }

        protected Dictionary<Guid, Func<int, bool>> OptionFiltersMap { get; } = new Dictionary<Guid, Func<int, bool>>();

        public Dictionary<Guid, Func<IExpressionExecutableV10, bool>> LinkedOptionFiltersMap = new Dictionary<Guid, Func<IExpressionExecutableV10, bool>>();

        public List<Guid> LinkedQuestions { get; private set; } = new List<Guid>();

        protected AbstractConditionalLevelInstanceV10(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
        }

        protected AbstractConditionalLevelInstanceV10(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies,
            IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.Quest = properties;
        }

        protected abstract Guid[] GetRosterScopeIds(Guid rosterId);

        protected abstract Guid GetQuestionnaireId();

        public abstract Guid[] GetRosterIdsThisScopeConsistOf();

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

        private bool GetOptionFilterResult(Func<int, bool> optionFilter, int optionValue)
        {
            try
            {
                return optionFilter(optionValue);
            }
            catch
            {
                return false;
            }
        }

        private bool GetLinkedFilterResult(Func<IExpressionExecutableV10, bool> linkedFilter, IExpressionExecutableV10 scope)
        {
            try
            {
                return linkedFilter(scope);
            }
            catch
            {
                return false;
            }
        }

        public virtual void SetRostersRemover(Action<Identity[], Guid, decimal> removeRosterInstances)
        {
            this.RemoveRosterInstances = removeRosterInstances;
        }

        public virtual void SetStructuralChangesCollector(StructuralChanges structuralChangesCollector)
        {
            this.StructuralChangesCollector = structuralChangesCollector;
        }

        protected virtual Action AnswerVerifier(Guid[] rosterScope, Guid itemId, Func<decimal[][]> getAnswer, Action<decimal[][]> setAnswer)
        {
            return () =>
            {
                var previousAnswers = getAnswer();

                if (previousAnswers == null)
                    return;

                if (previousAnswers.Any(rosterVector => !DoesRosterExist(rosterScope.Shrink(), rosterScope.Last(), rosterVector)))
                {
                    setAnswer(null);
                }
            };
        }

        protected virtual Action AnswerVerifier(Guid[] rosterScope, Guid itemId, Func<decimal[]> getAnswer, Action<decimal[]> setAnswer)
        {
            return () =>
            {
                var previousAnswer = getAnswer();

                if (previousAnswer == null)
                    return;

                if (DoesRosterExist(rosterScope.Shrink(), rosterScope.Last(), previousAnswer))
                    return;

                setAnswer(null);
            };
        }

        private bool DoesRosterExist(Guid[] parentRosterScope, Guid rosterSizeQuestionId, decimal[] rosterVector)
        {
            var rosterKey = Util.GetRosterKey(parentRosterScope, rosterVector);
            var rosters = this.GetInstances(rosterKey, rosterSizeQuestionId);
            return rosters != null && rosters.Any(x => x.RosterVector.SequenceEqual(rosterVector));
        }

        protected virtual Action AnswerVerifier(Func<int, bool> optionFilter, Guid itemId, Func<decimal?> getAnswer, Action<decimal?> setAnswer)
        {
            return () =>
            {
                var previousAnswer = getAnswer();
                if (previousAnswer.HasValue && GetOptionFilterResult(optionFilter, Convert.ToInt32(previousAnswer.Value)) == false)
                {
                    setAnswer(null);
                    this.StructuralChangesCollector.AddChangedSingleQuestion(new Identity(itemId, RosterVector), null);
                }
            };
        }

        protected virtual Action AnswerVerifier(Func<int, bool> optionFilter, Guid itemId, Func<decimal[]> getAnswer, Action<decimal[]> setAnswer)
        {
            return () =>
            {
                decimal[] previousAnswer = getAnswer();
                if (previousAnswer == null || previousAnswer.Length == 0)
                    return;

                var actualAnswer = previousAnswer.Where(selectedOption =>
                    GetOptionFilterResult(optionFilter, Convert.ToInt32(selectedOption)))
                    .ToArray();

                var wereSomeOptionsRemoved = previousAnswer.Length > actualAnswer.Length;
                if (wereSomeOptionsRemoved)
                {
                    setAnswer(actualAnswer);
                    this.StructuralChangesCollector.AddChangedMultiQuestion(new Identity(itemId, RosterVector), actualAnswer.Select(Convert.ToInt32).ToArray());

                    foreach (var rowcode in previousAnswer.Except(actualAnswer))
                    {
                        RemoveRosterInstances(RosterKey, itemId, rowcode);
                    }
                }
            };
        }

        protected virtual Action AnswerVerifier(Func<int, bool> optionFilter, Guid itemId, Func<YesNoAnswers> getAnswer, Action<YesNoAnswers> setAnswer)
        {
            return () =>
            {
                YesNoAnswers previousAnswer = getAnswer();
                if (previousAnswer == null || (previousAnswer.Yes.Length == 0 && previousAnswer.No.Length == 0))
                    return;

                var actualYesAnswers = previousAnswer.Yes.Where(selectedOption =>
                    GetOptionFilterResult(optionFilter, Convert.ToInt32(selectedOption)))
                    .ToArray();

                var actualNoAnswers = previousAnswer.No.Where(selectedOption =>
                    GetOptionFilterResult(optionFilter, Convert.ToInt32(selectedOption)))
                    .ToArray();

                var wereSomeYesOptionsRemoved = previousAnswer.Yes.Length > actualYesAnswers.Length;
                var wereSomeNoOptionsRemoved = previousAnswer.No.Length > actualNoAnswers.Length;
                if (wereSomeYesOptionsRemoved || wereSomeNoOptionsRemoved)
                {
                    var actualYesNoAnswersOnly = new YesNoAnswersOnly(actualYesAnswers, actualNoAnswers);
                    setAnswer(new YesNoAnswers(previousAnswer.All, actualYesNoAnswersOnly));
                    this.StructuralChangesCollector.AddChangedYesNoQuestion(new Identity(itemId, RosterVector), actualYesNoAnswersOnly);
                }

                if (wereSomeYesOptionsRemoved)
                {
                    foreach (var rowcode in previousAnswer.Yes.Except(actualYesAnswers))
                    {
                        RemoveRosterInstances(RosterKey, itemId, rowcode);
                    }
                }
            };
        }

        protected new Action Verifier(Func<bool> isEnabled, Guid itemId, ConditionalState questionState)
        {
            return () =>
            {
                if (questionState.State == State.Disabled)
                    return;

                questionState.State = this.GetConditionExpressionState(isEnabled);

                this.UpdateAllNestedItemsStateAndPropagateStateOnRosters(itemId, this.StructuralDependencies, questionState.State);
            };
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
                    var rosterScope = this.GetRosterScopeIds(id);

                    var rosters = this.GetNestedRostersBySourceQuestionId(rosterScope.Last());

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

        private IEnumerable<IExpressionExecutableV10> GetNestedRostersBySourceQuestionId(Guid rosterSorceQuestionId)
        {
            var isQuestionnaireLevel = this.RosterKey.Length == 1 && this.RosterKey[0].Id == this.GetQuestionnaireId();

            var rosterKey = isQuestionnaireLevel
                ? new Identity[0]
                : this.RosterKey;

            var rosters = this.GetInstances(rosterKey, rosterSorceQuestionId);
            return rosters;
        }

        public IEnumerable<CategoricalOption> FilterOptionsForQuestion(Guid questionId, IEnumerable<CategoricalOption> options)
        {
            if (!OptionFiltersMap.ContainsKey(questionId))
            {
                foreach (var option in options)
                    yield return option;

                yield break;
            }
            
            var filter = OptionFiltersMap[questionId];
            foreach (var option in options)
            {
                var isOptionSatisfyFilter = GetOptionFilterResult(filter, option.Value);
                if (isOptionSatisfyFilter)
                {
                    yield return option;
                }
            }
        }

        public LinkedQuestionFilterResult ExecuteLinkedQuestionFilter(IExpressionExecutableV10 currentScope, Guid questionId)
        {
            if (!this.LinkedOptionFiltersMap.ContainsKey(questionId))
                return null;
            
            var linkedQuestionFilter = this.LinkedOptionFiltersMap[questionId];
            var isEnabled = this.GetLinkedFilterResult(linkedQuestionFilter, currentScope);
            return new LinkedQuestionFilterResult
            {
                Enabled = isEnabled,
                LinkedQuestionId = questionId,
                RosterKey = this.RosterKey
            };
        }
    }
}