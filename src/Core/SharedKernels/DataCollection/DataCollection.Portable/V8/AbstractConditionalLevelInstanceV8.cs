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

        protected HashSet<Guid> ValidStaticTexts = new HashSet<Guid>();

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
            this.Quest = properties;
        }

        protected virtual void FillEnablementStates(Dictionary<Guid, ConditionalState> enablementStates)
        {
            foreach (var state in this.EnablementStates)
            {
                var originalState = enablementStates[state.Key];
                state.Value.PreviousState = originalState.PreviousState;
                state.Value.State = originalState.State;
            }
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
            => (x, y) => getInstancesV7(x, y)?.Cast<IExpressionExecutableV8>();

        public abstract IExpressionExecutableV8 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstances);

        public new IExpressionExecutableV8 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        protected virtual IDictionary<Guid, IReadOnlyList<FailedValidationCondition>> FailedValidations => this.InvalidAnsweredFailedValidations;

        
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

        public void DeclareStaticTextValid(Guid staticTextId)
        {
            this.ValidStaticTexts.Add(staticTextId);
            this.FailedValidations.Remove(staticTextId);
        }

        public void ApplyStaticTextFailedValidations(Guid staticTextId, IReadOnlyList<FailedValidationCondition> failedValidations)
        {
            this.ValidStaticTexts.Remove(staticTextId);
            this.FailedValidations[staticTextId] = failedValidations;
        }

        protected new ValidityChanges ExecuteValidations()
        {
            var questionsToBeValid = new List<Identity>();
            var questionsToBeInvalid = new List<Identity>();
            var failedQuestionsValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();

            var staticTextsToBeValid = new List<Identity>();
            var failedStaticTextsValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();

            this.ExecuteQuestionsValidation(questionsToBeValid, questionsToBeInvalid, failedQuestionsValidationConditions);
            this.ExecuteStaticTextsValidations(staticTextsToBeValid, failedStaticTextsValidationConditions);

            return new ValidityChanges(
                questionsToBeValid, questionsToBeInvalid, failedQuestionsValidationConditions,
                staticTextsToBeValid, failedStaticTextsValidationConditions);
        }

        private void ExecuteStaticTextsValidations(List<Identity> staticTextsToBeValid, Dictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedStaticTextsValidationConditions)
        {
            foreach (var validationExpressionDescription in this.ValidationExpressionDescriptions.Where(x => x.Value.IsFromStaticText))
            {
                try
                {
                    // do not validate disabled static texts
                    Guid staticTextId = validationExpressionDescription.Key.Id;
                    if (this.EnablementStates.ContainsKey(staticTextId) &&
                        this.EnablementStates[staticTextId].State == State.Disabled)
                        continue;

                    bool isValid;
                    List<FailedValidationCondition> invalids = new List<FailedValidationCondition>();
                    if (validationExpressionDescription.Value.PreexecutionCheck.Invoke())
                    {
                        isValid = true;
                    }
                    else
                    {
                        foreach (var validation in validationExpressionDescription.Value.Validations)
                        {
                            try
                            {
                                if (!validation.Value.Invoke())
                                    invalids.Add(new FailedValidationCondition() {FailedConditionIndex = validation.Key});
                            }
                            catch
                            {
                                invalids.Add(new FailedValidationCondition() {FailedConditionIndex = validation.Key});
                            }
                        }

                        isValid = !invalids.Any();
                    }

                    if (isValid && !this.ValidStaticTexts.Contains(staticTextId))
                    {
                        staticTextsToBeValid.Add(validationExpressionDescription.Key);
                    }
                    else if (!isValid)
                    {
                        // no changes in invalid validations
                        // do not raise
                        if (this.FailedValidations.ContainsKey(staticTextId) &&
                            (this.FailedValidations[staticTextId].Count == invalids.Count) &&
                            !this.FailedValidations[staticTextId].Except(invalids).Any())
                            continue;
                        else // first or invalid old events support, raising a new one 
                        {
                            failedStaticTextsValidationConditions.Add(validationExpressionDescription.Key, invalids);
                        }
                    }
                }
                catch
                {
                    // failed to execute are treated as valid
                }
            }
        }

        private void ExecuteQuestionsValidation(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid,
            Dictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedQuestionsValidationConditions)
        {
            foreach (var validationExpressionDescription in this.ValidationExpressionDescriptions.Where(x => x.Value.IsFromQuestion))
            {
                try
                {
                    // do not validate disabled questions
                    Guid questionId = validationExpressionDescription.Key.Id;
                    if (this.EnablementStates.ContainsKey(questionId) &&
                        this.EnablementStates[questionId].State == State.Disabled)
                        continue;

                    bool isValid;
                    List<FailedValidationCondition> invalids = new List<FailedValidationCondition>();
                    if (validationExpressionDescription.Value.PreexecutionCheck.Invoke())
                    {
                        isValid = true;
                    }
                    else
                    {
                        foreach (var validation in validationExpressionDescription.Value.Validations)
                        {
                            try
                            {
                                if (!validation.Value.Invoke())
                                    invalids.Add(new FailedValidationCondition() {FailedConditionIndex = validation.Key});
                            }
                            catch
                            {
                                invalids.Add(new FailedValidationCondition() {FailedConditionIndex = validation.Key});
                            }
                        }

                        isValid = !invalids.Any();
                    }

                    if (isValid && !this.ValidAnsweredQuestions.Contains(questionId))
                    {
                        questionsToBeValid.Add(validationExpressionDescription.Key);
                    }
                    else if (!isValid) // && !this.InvalidAnsweredQuestions.Contains(questionId)
                    {
                        // no changes in invalid validations
                        // do not raise
                        if (this.FailedValidations.ContainsKey(questionId) &&
                            (this.FailedValidations[questionId].Count == invalids.Count) &&
                            !this.FailedValidations[questionId].Except(invalids).Any())
                            continue;
                        else // first or invalid old events support, raising a new one 
                        {
                            questionsToBeInvalid.Add(validationExpressionDescription.Key);
                            failedQuestionsValidationConditions.Add(validationExpressionDescription.Key, invalids);
                        }
                    }
                }
                catch
                {
                    // failed to execute are treated as valid
                    questionsToBeInvalid.Add(validationExpressionDescription.Key);
                }
            }
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