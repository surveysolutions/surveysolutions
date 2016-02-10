using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.V4;
using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection.V6
{
    public abstract class AbstractConditionalLevelInstanceV6<T> : AbstractConditionalLevelInstanceV5<T>
        where T : IExpressionExecutableV6, IExpressionExecutableV5, IExpressionExecutableV2, IExpressionExecutable
    {

        protected Dictionary<Guid, IReadOnlyList<FailedValidationCondition>> InvalidAnsweredFailedValidations = new Dictionary<Guid, IReadOnlyList<FailedValidationCondition>>();

        protected new Func<Identity[], Guid, IEnumerable<IExpressionExecutableV6>> GetInstances { get; private set; }

        protected new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV6>> RosterGenerators { get;
            set; }

        protected Dictionary<Identity, ValidationDescription> ValidationExpressionDescriptions =
            new Dictionary<Identity, ValidationDescription>();

        protected AbstractConditionalLevelInstanceV6(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid,
                IEnumerable<IExpressionExecutableV6>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.RosterGenerators = new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutableV6>>();
        }

        protected AbstractConditionalLevelInstanceV6(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid,
                IEnumerable<IExpressionExecutableV6>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies,
            Dictionary<Guid, Guid[]> structuralDependencies, IInterviewProperties properties)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.Quest = properties;
        }


        public override IExpressionExecutableV2 CopyMembers(
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV2>> getInstances)
        {
            return null;
        }

        public override IExpressionExecutableV5 CopyMembers(
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV5>> getInstances)
        {
            return null;
        }

        public abstract IExpressionExecutableV6 CopyMembers(
            Func<Identity[], Guid, IEnumerable<IExpressionExecutableV6>> getInstances);

        public new IExpressionExecutableV6 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector,
            Identity[] rosterIdentityKey)
        {
            return this.RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        public new void DeclareAnswerValid(Guid questionId)
        {
            this.ValidAnsweredQuestions.Add(questionId);
            this.InvalidAnsweredQuestions.Remove(questionId);
            this.InvalidAnsweredFailedValidations.Remove(questionId);
        }

        public void ApplyFailedValidations(Guid questionId, IReadOnlyList<FailedValidationCondition> failedValidations)
        {
            this.ValidAnsweredQuestions.Remove(questionId);
            this.InvalidAnsweredQuestions.Add(questionId);

            this.InvalidAnsweredFailedValidations[questionId] = failedValidations;
        }

        protected ValidityChanges ExecuteValidations()
        {
            var questionsToBeValid = new List<Identity>();
            var questionsToBeInvalid = new List<Identity>();
            var failedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();

            foreach (var validationExpressionDescription in this.ValidationExpressionDescriptions)
            {
                try
                {
                    // do not validate disabled questions
                    Guid questionId = validationExpressionDescription.Key.Id;
                    if (this.EnablementStates.ContainsKey(questionId) && this.EnablementStates[questionId].State == State.Disabled)
                        continue;

                    bool isValid;
                    List<FailedValidationCondition> invalids = new List<FailedValidationCondition>();
                    if (validationExpressionDescription.Value.PreexecutionCheck())
                        isValid = true;

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
                                invalids.Add(new FailedValidationCondition() { FailedConditionIndex = validation.Key });
                            }
                        }

                        isValid = !invalids.Any();
                    }

                    if (isValid && !this.ValidAnsweredQuestions.Contains(questionId))
                    {
                        questionsToBeValid.Add(validationExpressionDescription.Key);
                    }
                    else if (!isValid)// && !this.InvalidAnsweredQuestions.Contains(questionId)
                    {
                        // no changes in invalid validations
                        // do not raise
                        if (this.InvalidAnsweredFailedValidations.ContainsKey(questionId) &&
                            (this.InvalidAnsweredFailedValidations[questionId].Count == invalids.Count) &&
                            !this.InvalidAnsweredFailedValidations[questionId].Except(invalids).Any())
                            continue;
                        else // first or invalid old events support, raising a new one 
                        {
                            questionsToBeInvalid.Add(validationExpressionDescription.Key);
                            failedValidationConditions.Add(validationExpressionDescription.Key, invalids);
                        }
                    }
                }
#pragma warning disable
                catch (Exception ex)
                {
                    // failed to execute are treated as valid
                    questionsToBeInvalid.Add(validationExpressionDescription.Key);
                }
#pragma warning restore


            }

            return new ValidityChanges(questionsToBeValid, questionsToBeInvalid, failedValidationConditions);
        }
    }
}