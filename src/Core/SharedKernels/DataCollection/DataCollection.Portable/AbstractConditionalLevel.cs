using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.V2.CustomFunctions;

namespace WB.Core.SharedKernels.DataCollection
{
    public abstract class AbstractConditionalLevel<T> : AbstractConditionalLevelInstanceFunctions where T : IExpressionExecutable
    {
        public decimal[] RosterVector { get; private set; }
        public Identity[] RosterKey { get; private set; }

        protected Dictionary<Guid, ConditionalState> EnablementStates { get; private set; }

        protected Dictionary<Guid, Action<string>> QuestionStringUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<long?>> QuestionLongUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<decimal?>> QuestionDecimalUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<double?>> QuestionDoubleUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<DateTime?>> QuestionDateTimeUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<decimal[]>> QuestionDecimal1DArrayUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<decimal[][]>> QuestionDecimal2DArrayUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<Tuple<decimal, string>[]>> QuestionTupleArrayUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<GeoLocation>> QuestionGpsUpdateMap { get; private set; }

        protected Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutable>> RosterGenerators { get; set; }

        protected Dictionary<Identity, Func<bool>[]> ValidationExpressions = new Dictionary<Identity, Func<bool>[]>();

        public decimal[] me
        {
            get { return this.RosterVector; }
        }

        public int GetLevel()
        {
            return RosterVector.Length;
        }

        protected HashSet<Guid> ValidAnsweredQuestions = new HashSet<Guid>();
        protected HashSet<Guid> InvalidAnsweredQuestions = new HashSet<Guid>();

        protected Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> GetInstances { get; private set; }

        protected Dictionary<Guid, Guid[]> ConditionalDependencies { get; set; }
        protected Dictionary<Guid, Guid[]> StructuralDependencies { get; set; }

        protected abstract IEnumerable<Action> ConditionExpressions { get; }

        protected AbstractConditionalLevel(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structuralDependencies)
        {
            this.GetInstances = getInstances;
            this.RosterVector = rosterVector;
            this.RosterKey = rosterKey;
            this.EnablementStates = new Dictionary<Guid, ConditionalState>();
            this.ConditionalDependencies = conditionalDependencies;
            this.StructuralDependencies = structuralDependencies;

            this.QuestionStringUpdateMap = new Dictionary<Guid, Action<string>>();
            this.QuestionLongUpdateMap = new Dictionary<Guid, Action<long?>>();
            this.QuestionDateTimeUpdateMap = new Dictionary<Guid, Action<DateTime?>>();
            this.QuestionDecimal1DArrayUpdateMap = new Dictionary<Guid, Action<decimal[]>>();
            this.QuestionDecimal2DArrayUpdateMap = new Dictionary<Guid, Action<decimal[][]>>();
            this.QuestionDecimalUpdateMap = new Dictionary<Guid, Action<decimal?>>();
            this.QuestionDoubleUpdateMap = new Dictionary<Guid, Action<double?>>();
            this.QuestionGpsUpdateMap = new Dictionary<Guid, Action<GeoLocation>>();
            this.QuestionTupleArrayUpdateMap = new Dictionary<Guid, Action<Tuple<decimal, string>[]>>();

            this.RosterGenerators = new Dictionary<Guid, Func<decimal[], Identity[], IExpressionExecutable>>();
        }

        public IExpressionExecutable CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
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

        protected void DisableAllDependentQuestions(Guid itemId, Dictionary<Guid, Guid[]> conditionalDependencies)
        {
            if (!conditionalDependencies.ContainsKey(itemId) || !conditionalDependencies[itemId].Any()) return;

            var stack = new Queue<Guid>(conditionalDependencies[itemId]);
            while (stack.Any())
            {
                var id = stack.Dequeue();

                if (this.EnablementStates.ContainsKey(id))
                {
                    this.EnablementStates[id].State = State.Disabled;
                }

                if (conditionalDependencies.ContainsKey(id) && conditionalDependencies[id].Any())
                {
                    foreach (var dependentQuestionId in conditionalDependencies[id])
                    {
                        stack.Enqueue(dependentQuestionId);
                    }
                }
            }
        }

        protected void UpdateAllNestedItemsState(Guid itemId, Dictionary<Guid, Guid[]> structureDependencies, State state)
        {
            if (!structureDependencies.ContainsKey(itemId) || !structureDependencies[itemId].Any()) return;

            var stack = new Queue<Guid>(structureDependencies[itemId]);
            while (stack.Any())
            {
                var id = stack.Dequeue();

                if (this.EnablementStates.ContainsKey(id))
                {
                    this.EnablementStates[id].State = state;
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

        protected void AddUpdaterToMap(Guid id, Action<string> action)
        {
            this.QuestionStringUpdateMap.Add(id, action);
        }


        protected void AddUpdaterToMap(Guid id, Action<long?> action)
        {
            this.QuestionLongUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<DateTime?> action)
        {
            this.QuestionDateTimeUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<decimal[]> action)
        {
            this.QuestionDecimal1DArrayUpdateMap.Add(id, action);
        }


        protected void AddUpdaterToMap(Guid id, Action<decimal[][]> action)
        {
            this.QuestionDecimal2DArrayUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<decimal?> action)
        {
            this.QuestionDecimalUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<double?> action)
        {
            this.QuestionDoubleUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<GeoLocation> action)
        {
            this.QuestionGpsUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<Tuple<decimal, string>[]> action)
        {
            this.QuestionTupleArrayUpdateMap.Add(id, action);
        }

        protected Action Verifier(Func<bool> isEnabled, Guid itemId, ConditionalState questionState)
        {
            return () =>
            {
                if (questionState.State == State.Disabled)
                    return;

                questionState.State = this.GetConditionExpressionState(isEnabled);

                this.UpdateAllNestedItemsState(itemId, this.StructuralDependencies,
                    questionState.State);
            };
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            foreach (var state in this.EnablementStates.Values)
            {
                state.PreviousState = state.State;
                state.State = State.Unknown;
            }
        }

        public void EvaluateConditions(out List<Identity> questionsToBeEnabled, out List<Identity> questionsToBeDisabled,
            out List<Identity> groupsToBeEnabled, out List<Identity> groupsToBeDisabled)
        {
            foreach (Action verifier in this.ConditionExpressions)
            {
                verifier();
            }

            questionsToBeEnabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Question)
                .Where(StateChangedToEnabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            questionsToBeDisabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Question)
                .Where(StateChangedToDisabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            groupsToBeEnabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Group)
                .Where(StateChangedToEnabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();

            groupsToBeDisabled = this.EnablementStates.Values
                .Where(x => x.Type == ItemType.Group)
                .Where(StateChangedToDisabled)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToList();
        }

        private static bool StateChangedToEnabled(ConditionalState state)
        {
            if (state.State == State.Unknown && state.PreviousState == State.Disabled)
            {
                return false;
            }

            bool isCurrentStateEnabled = state.State == State.Enabled || state.State == State.Unknown;
            bool isPreviousStateEnabled = state.PreviousState == State.Enabled || state.PreviousState == State.Unknown;

            return isCurrentStateEnabled && !isPreviousStateEnabled;
        }


        private static bool StateChangedToDisabled(ConditionalState state)
        {
            bool isCurrentStateDisabled = state.State == State.Disabled;
            bool isPreviousStateDisabled = state.PreviousState == State.Disabled;

            return isCurrentStateDisabled && !isPreviousStateDisabled;
        }

        public void DeclareAnswerValid(Guid questionId)
        {
            this.ValidAnsweredQuestions.Add(questionId);
            this.InvalidAnsweredQuestions.Remove(questionId);
        }

        public void DeclareAnswerInvalid(Guid questionId)
        {
            this.InvalidAnsweredQuestions.Add(questionId);
            this.ValidAnsweredQuestions.Remove(questionId);
        }

        public void DisableQuestion(Guid questionId)
        {
            if (this.EnablementStates.ContainsKey(questionId))
                this.EnablementStates[questionId].State = State.Disabled;
        }

        public void EnableQuestion(Guid questionId)
        {
            if (this.EnablementStates.ContainsKey(questionId))
                this.EnablementStates[questionId].State = State.Enabled;
        }

        public void DisableGroup(Guid groupId)
        {
            if (this.EnablementStates.ContainsKey(groupId))
                this.EnablementStates[groupId].State = State.Disabled;
        }

        public void EnableGroup(Guid groupId)
        {
            if (this.EnablementStates.ContainsKey(groupId))
                this.EnablementStates[groupId].State = State.Enabled;
        }

        public Identity[] GetRosterKey()
        {
            return this.RosterKey;
        }

        protected bool IsEnabledIfParentIs()
        {
            return true;
        }

        protected bool IsAnswered(string answer)
        {
            return !string.IsNullOrWhiteSpace(answer);
        }

        protected bool IsAnswered<TY>(TY? answer) where TY : struct
        {
            return answer.HasValue;
        }

        protected bool IsAnswered<TY>(TY answer) where TY : class
        {
            return answer != null;
        }

        protected bool IsAnswered<TY>(TY[] answer)
        {
            return answer != null && answer.Length != 0;
        }

        protected bool IsAnswered(Tuple<decimal, string>[] answer)
        {
            return answer != null && answer.Any();
        }

        public void CalculateConditionChanges(out List<Identity> questionsToBeEnabled, out List<Identity> questionsToBeDisabled,
            out List<Identity> groupsToBeEnabled, out List<Identity> groupsToBeDisabled)
        {
            this.EvaluateConditions(out questionsToBeEnabled, out questionsToBeDisabled, out groupsToBeEnabled, out groupsToBeDisabled);
        }

        public void UpdateMediaAnswer(Guid questionId, string answer)
        {
            if (this.QuestionStringUpdateMap.ContainsKey(questionId))
                this.QuestionStringUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateTextAnswer(Guid questionId, string answer)
        {
            if (this.QuestionStringUpdateMap.ContainsKey(questionId))
                this.QuestionStringUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateNumericIntegerAnswer(Guid questionId, long? answer)
        {
            if (this.QuestionLongUpdateMap.ContainsKey(questionId))
                this.QuestionLongUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateNumericRealAnswer(Guid questionId, double? answer)
        {
            if (this.QuestionDoubleUpdateMap.ContainsKey(questionId))
                this.QuestionDoubleUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateDateTimeAnswer(Guid questionId, DateTime? answer)
        {
            if (this.QuestionDateTimeUpdateMap.ContainsKey(questionId))
                this.QuestionDateTimeUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, string answer)
        {
            if (this.QuestionStringUpdateMap.ContainsKey(questionId))
                this.QuestionStringUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal? answer)
        {
            if (this.QuestionDecimalUpdateMap.ContainsKey(questionId))
                this.QuestionDecimalUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] answer)
        {
            if (this.QuestionDecimal1DArrayUpdateMap.ContainsKey(questionId))
                this.QuestionDecimal1DArrayUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateGeoLocationAnswer(Guid questionId, double latitude, double longitude, double precision, double altitude)
        {
            if (this.QuestionGpsUpdateMap.ContainsKey(questionId))
                this.QuestionGpsUpdateMap[questionId].Invoke(new GeoLocation(latitude, longitude, precision, altitude));
        }

        public void UpdateTextListAnswer(Guid questionId, Tuple<decimal, string>[] answers)
        {
            if (this.QuestionTupleArrayUpdateMap.ContainsKey(questionId))
                this.QuestionTupleArrayUpdateMap[questionId].Invoke(answers);
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] selectedPropagationVector)
        {
            if (this.QuestionDecimal1DArrayUpdateMap.ContainsKey(questionId))
                this.QuestionDecimal1DArrayUpdateMap[questionId].Invoke(selectedPropagationVector);
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[][] selectedPropagationVectors)
        {
            if (this.QuestionDecimal2DArrayUpdateMap.ContainsKey(questionId))
                this.QuestionDecimal2DArrayUpdateMap[questionId].Invoke(selectedPropagationVectors);
        }

        protected void Validate(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid)
        {
            questionsToBeValid = new List<Identity>();
            questionsToBeInvalid = new List<Identity>();

            foreach (var validationExpression in this.ValidationExpressions)
            {
                try
                {
                    // do not validate disabled questions
                    Guid questionId = validationExpression.Key.Id;
                    if (this.EnablementStates.ContainsKey(questionId) &&
                        this.EnablementStates[questionId].State == State.Disabled) continue;

                    bool isValid = validationExpression.Value.All(func => func());

                    if (isValid && !this.ValidAnsweredQuestions.Contains(questionId))
                        questionsToBeValid.Add(validationExpression.Key);
                    else if (!isValid && !this.InvalidAnsweredQuestions.Contains(questionId))
                        questionsToBeInvalid.Add(validationExpression.Key);
                }
#pragma warning disable
                catch (Exception ex)
                {
                    // failed to execute are treated as valid
                    questionsToBeValid.Add(validationExpression.Key);
                }
#pragma warning restore
            }
        }
    }
}