using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public interface IValidatable
    {
        IValidatable CopyMembers();

        Identity[] GetRosterKey();
        void SetParent(IValidatable parentLevel);
        IValidatable GetParent();

        void CalculateValidationChanges(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);

        void CalculateConditionChanges(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled,
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);

        void DisableQuestion(Guid questionId);
        void EnableQuestion(Guid questionId);

        void DisableGroup(Guid groupId);
        void EnableGroup(Guid groupId);

        void DeclareAnswerValid(Guid questionId);
        void DeclareAnswerInvalid(Guid questionId);

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
        
        public decimal[] me
        {
            get { return this.RosterVector; }
        }

        protected HashSet<Guid> ValidAnsweredQuestions = new HashSet<Guid>();
        protected HashSet<Guid> InvalidAnsweredQuestions = new HashSet<Guid>();

        protected Func<Identity[], IEnumerable<IValidatable>> GetInstances { get; private set;}

        protected abstract IEnumerable<Action> ConditionExpressions { get; }

        protected AbstractConditionalLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances)
        {
            this.GetInstances = getInstances;
            this.RosterVector = rosterVector;
            this.RosterKey = rosterKey;
            this.EnablementStates = new Dictionary<Guid, ConditionalState>();
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

        protected void DisableAllDependentQuestions(Guid itemId)
        {
            if (!IdOf.conditionalDependencies.ContainsKey(itemId) || !IdOf.conditionalDependencies[itemId].Any()) return;

            var stack = new Queue<Guid>(IdOf.conditionalDependencies[itemId]);
            while (stack.Any())
            {
                var id = stack.Dequeue();

                if (this.EnablementStates.ContainsKey(id))
                {
                    this.EnablementStates[id].State = State.Disabled;
                }

                if (IdOf.conditionalDependencies.ContainsKey(id) && IdOf.conditionalDependencies[id].Any())
                {
                    foreach (var dependentQuestionId in IdOf.conditionalDependencies[id])
                    {
                        stack.Enqueue(dependentQuestionId);
                    }
                }
            }
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
                    this.DisableAllDependentQuestions(questionId);
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
            ValidAnsweredQuestions.Add(questionId);
            InvalidAnsweredQuestions.Remove(questionId);
        }

        public void DeclareAnswerInvalid(Guid questionId)
        {
            InvalidAnsweredQuestions.Add(questionId);
            ValidAnsweredQuestions.Remove(questionId);
        }

        public void DisableQuestion(Guid questionId)
        {
            if (EnablementStates.ContainsKey(questionId))
                EnablementStates[questionId].State = State.Disabled;
        }

        public void EnableQuestion(Guid questionId)
        {
            if (EnablementStates.ContainsKey(questionId))
                EnablementStates[questionId].State = State.Enabled;
        }

        public void DisableGroup(Guid groupId)
        {
            if (EnablementStates.ContainsKey(groupId))
                EnablementStates[groupId].State = State.Disabled;
        }

        public void EnableGroup(Guid groupId)
        {
            if (EnablementStates.ContainsKey(groupId))
                EnablementStates[groupId].State = State.Enabled;
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
    }

    public abstract class AbstractRosterLevel<T> : AbstractConditionalLevel<T> where T : IValidatable
    {
        protected AbstractRosterLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances) : base(rosterVector, rosterKey, getInstances) { }
        
        public decimal index
        {
            get { return this.RosterVector.Last(); }
        }


        protected Dictionary<Identity, Func<bool>[]> validationExpressions = new Dictionary<Identity, Func< bool>[]>();

        protected void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var validationExpression in validationExpressions)
            {
                try
                {
                    // do not validate disabled questions
                    Guid questionId = validationExpression.Key.Id;
                    if (this.EnablementStates.ContainsKey(questionId) &&
                        this.EnablementStates[questionId].State != State.Enabled) continue;

                    var isValid = validationExpression.Value.Aggregate(true, (current, validator) => current && validator());

                    if (isValid && !ValidAnsweredQuestions.Contains(questionId))
                        questionsToBeValid.Add(validationExpression.Key);

                    if (!isValid && !InvalidAnsweredQuestions.Contains(questionId))
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
