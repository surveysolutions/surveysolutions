using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IValidatable
    {
        IValidatable CopyMembers();

        Identity[] GetRosterKey();
        void SetParent(IValidatable parentLevel);
        IValidatable GetParent();

        IValidatable CreateChildRosterInstance(Guid rosterId,decimal[] rosterVector, Identity[] rosterIdentityKey);

        void CalculateValidationChanges(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);

        void CalculateConditionChanges(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled,
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);

        void DisableQuestion(Guid questionId);
        void EnableQuestion(Guid questionId);

        void DisableGroup(Guid groupId);
        void EnableGroup(Guid groupId);

        void DeclareAnswerValid(Guid questionId);
        void DeclareAnswerInvalid(Guid questionId);

        void UpdateIntAnswer(Guid questionId, long answer);
        void UpdateDecimalAnswer(Guid questionId, decimal answer);
        void UpdateDateTimeAnswer(Guid questionId, DateTime answer);
        void UpdateTextAnswer(Guid questionId, string answer);
        void UpdateQrBarcodeAnswer(Guid questionId, string answer);
        void UpdateSingleOptionAnswer(Guid questionId, decimal answer);
        void UpdateMultiOptionAnswer(Guid questionId, decimal[] answer);
        void UpdateGeoLocationAnswer(Guid questionId, double latitude, double longitude, double precision);
        void UpdateTextListAnswer(Guid questionId, Tuple<decimal, string>[] answers);
        void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] selectedPropagationVector);
        void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[][] selectedPropagationVectors);

    }

    //could be replaces with bool?

    public enum State
    {
        Unknown = 0,
        Enabled = 1,
        Disabled = 2
    }

    public enum ItemType
    {
        Question = 1,
        Group = 10
    }

    public class ConditionalState
    {
        public ConditionalState(Guid itemId, ItemType type = ItemType.Question, State state = State.Enabled, State previousState = State.Enabled)
        {
            this.Type = type;
            this.ItemId = itemId;
            this.State = state;
            this.PreviousState = previousState;
        }

        public Guid ItemId { get; set; }
        public ItemType Type { get; set; }
        public State State { get; set; }
        public State PreviousState { get; set; }
    }

    public abstract class AbstractConditionalLevel<T> where T : IValidatable
    {
        public decimal[] RosterVector { get; private set; }
        public Identity[] RosterKey { get; private set; }

        protected Dictionary<Guid, ConditionalState> EnablementStates {get; private set; }

        protected Dictionary<Guid, Action<string>> QuestionStringUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<long?>> QuestionLongUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<decimal?>> QuestionDecimalUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<DateTime?>> QuestionDateTimeUpdateMap { get; private set; }

        protected Dictionary<Guid, Action<decimal[]>> QuestionDecimal1DArrayUpdateMap { get; private set; }
        protected Dictionary<Guid, Action<decimal[][]>> QuestionDecimal2DArrayUpdateMap { get; private set; }

        protected Dictionary<Guid, Action<Tuple<decimal, string>[]>> QuestionTupleArrayUpdateMap { get; private set; }

        protected Dictionary<Guid, Action<double,double, double> > QuestionGPSUpdateMap { get; private set; }


        protected Dictionary<Guid, Func<decimal[], Identity[], IValidatable>> RosterGenerators { get; set; }

        public decimal[] me
        {
            get { return this.RosterVector; }
        }

        protected HashSet<Guid> ValidAnsweredQuestions = new HashSet<Guid>();
        protected HashSet<Guid> InvalidAnsweredQuestions = new HashSet<Guid>();

        protected Func<Identity[], IEnumerable<IValidatable>> GetInstances { get; private set;}

        protected Dictionary<Guid, Guid[]> ConditionalDependencies { get; set; }


        protected abstract IEnumerable<Action> ConditionExpressions { get; }

        protected AbstractConditionalLevel(decimal[] rosterVector, Identity[] rosterKey,
            Func<Identity[], IEnumerable<IValidatable>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies)
        {
            this.GetInstances = getInstances;
            this.RosterVector = rosterVector;
            this.RosterKey = rosterKey;
            this.EnablementStates = new Dictionary<Guid, ConditionalState>();
            this.ConditionalDependencies = conditionalDependencies;

            this.QuestionStringUpdateMap = new Dictionary<Guid, Action<string>>();
            this.QuestionLongUpdateMap = new Dictionary<Guid, Action<long?>>();
            this.QuestionDateTimeUpdateMap = new Dictionary<Guid, Action<DateTime?>>();
            this.QuestionDecimal1DArrayUpdateMap = new Dictionary<Guid, Action<decimal[]>>();
            this.QuestionDecimal2DArrayUpdateMap = new Dictionary<Guid, Action<decimal[][]>>();
            this.QuestionDecimalUpdateMap = new Dictionary<Guid, Action<decimal?>>();
            this.QuestionGPSUpdateMap = new Dictionary<Guid, Action<double, double, double>>();
            this.QuestionTupleArrayUpdateMap = new Dictionary<Guid, Action<Tuple<decimal, string>[]>>();

        }

        public IValidatable CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey)
        {
            return RosterGenerators[rosterId].Invoke(rosterVector, rosterIdentityKey);
        }

        private State RunConditionExpression(Func<bool> expression)
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

        protected void AddUpdaterToMap(Guid id, Action<string> action)
        {
            QuestionStringUpdateMap.Add(id, action);
        }


        protected void AddUpdaterToMap(Guid id, Action<long?> action)
        {
            QuestionLongUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<DateTime?> action)
        {
            QuestionDateTimeUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<decimal[]> action)
        {
            QuestionDecimal1DArrayUpdateMap.Add(id, action);
        }


        protected void AddUpdaterToMap(Guid id, Action<decimal[][]> action)
        {
            QuestionDecimal2DArrayUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<decimal?> action)
        {
            QuestionDecimalUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<double, double, double> action)
        {
            QuestionGPSUpdateMap.Add(id, action);
        }

        protected void AddUpdaterToMap(Guid id, Action<Tuple<decimal,string>[]> action)
        {
            QuestionTupleArrayUpdateMap.Add(id, action);
        }
        
        protected Action Verifier(Func<bool> isEnabled, Guid questionId, ConditionalState questionState)
        {
            return () =>
            {
                if (questionState.State == State.Disabled)
                    return;

                questionState.State = this.RunConditionExpression(isEnabled);

                if (questionState.State == State.Disabled)
                {
                    this.DisableAllDependentQuestions(questionId, this.ConditionalDependencies);
                }
            };
        }

        public void EvaluateConditions(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled,
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            foreach (var state in this.EnablementStates.Values)
            {
                state.PreviousState = state.State;
                state.State = State.Unknown;
            }

            foreach (Action verifier in this.ConditionExpressions)
            {
                verifier();
            }

            var questionsToBeEnabledArray = this.EnablementStates.Values
                .Where(x => x.State == State.Enabled && x.State != x.PreviousState && x.Type == ItemType.Question)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToArray();

            var questionsToBeDisabledArray = this.EnablementStates.Values
                .Where(x => x.State == State.Disabled && x.State != x.PreviousState && x.Type == ItemType.Question)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToArray();

            var groupsToBeEnabledArray = this.EnablementStates.Values
                .Where(x => x.State == State.Enabled && x.State != x.PreviousState && x.Type == ItemType.Group)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToArray();

            var groupsToBeDisabledArray = this.EnablementStates.Values
                .Where(x => x.State == State.Disabled && x.State != x.PreviousState && x.Type == ItemType.Group)
                .Select(x => new Identity(x.ItemId, this.RosterVector));

            questionsToBeEnabled.AddRange(questionsToBeEnabledArray);
            questionsToBeDisabled.AddRange(questionsToBeDisabledArray);
            groupsToBeEnabled.AddRange(groupsToBeEnabledArray);
            groupsToBeDisabled.AddRange(groupsToBeDisabledArray);
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

        protected bool IsEmptyAnswer(string answer)
        {
            return string.IsNullOrWhiteSpace(answer);
        }

        protected bool IsEmptyAnswer<TY>(TY? answer) where TY : struct
        {
            return answer.HasValue;
        }

        protected bool IsEmptyAnswer<TY>(TY[] answer) where TY : struct
        {
            return answer!=null && answer.Length > 0;
        }

        public void CalculateConditionChanges(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, List<Identity> groupsToBeEnabled,
            List<Identity> groupsToBeDisabled)
        {
            this.EvaluateConditions(questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled, groupsToBeDisabled);
        }

        public void UpdateTextAnswer(Guid questionId, string answer)
        {
            QuestionStringUpdateMap[questionId].Invoke(answer);
            
        }

        public void UpdateIntAnswer(Guid questionId, long answer)
        {
            QuestionLongUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateDecimalAnswer(Guid questionId, decimal answer)
        {
            QuestionDecimalUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateDateTimeAnswer(Guid questionId, DateTime answer)
        {
            QuestionDateTimeUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, string answer)
        {
            QuestionStringUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal answer)
        {
            QuestionDecimalUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] answer)
        {
            QuestionDecimal1DArrayUpdateMap[questionId].Invoke(answer);
        }

        public void UpdateGeoLocationAnswer(Guid questionId, double latitude, double longitude, double precision)
        {
            QuestionGPSUpdateMap[questionId].Invoke(latitude,longitude,precision);
        }

        public void UpdateTextListAnswer(Guid questionId, Tuple<decimal, string>[] answers)
        {
            QuestionTupleArrayUpdateMap[questionId].Invoke(answers);
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] selectedPropagationVector)
        {
            QuestionDecimal1DArrayUpdateMap[questionId].Invoke(selectedPropagationVector);
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[][] selectedPropagationVectors)
        {
            QuestionDecimal2DArrayUpdateMap[questionId].Invoke(selectedPropagationVectors);
        }
    }

    public abstract class AbstractRosterLevel<T> : AbstractConditionalLevel<T> where T : IValidatable
    {
        protected AbstractRosterLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies) 
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies) { }
        
        public decimal index
        {
            get { return this.RosterVector.Last(); }
        }

        protected Dictionary<Identity, Func<bool>[]> ValidationExpressions = new Dictionary<Identity, Func< bool>[]>();

        protected void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var validationExpression in this.ValidationExpressions)
            {
                try
                {
                    // do not validate disabled questions
                    Guid questionId = validationExpression.Key.Id;
                    if (this.EnablementStates.ContainsKey(questionId) &&
                        this.EnablementStates[questionId].State != State.Enabled) continue;

                    var isValid = validationExpression.Value.Aggregate(true, (current, validator) => current && validator());

                    if (isValid && !this.ValidAnsweredQuestions.Contains(questionId))
                        questionsToBeValid.Add(validationExpression.Key);

                    if (!isValid && !this.InvalidAnsweredQuestions.Contains(questionId))
                        questionsToBeInvalid.Add(validationExpression.Key);
                }
                catch (Exception)
                {
                    // failed to execute are treated as valid
                    questionsToBeValid.Add(validationExpression.Key);
                }
            }
        }
        
    }

}
