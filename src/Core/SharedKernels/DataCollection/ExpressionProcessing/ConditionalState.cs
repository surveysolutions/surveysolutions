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


        void DisableQuestion(Guid questionId);
        void EnableQuestion(Guid questionId);

        void DisableGroup(Guid groupId);
        void EnableGroup(Guid groupId);

        void DeclareAnswerValid(Guid questionId);
        void DeclareAnswerInvalid(Guid questionId);

    }

    public interface IValidatableRoster
    {
        void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
        
        void RunConditions(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled,
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);
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

        protected Dictionary<Guid, ConditionalState> EnablementStates = new Dictionary<Guid, ConditionalState>();

        protected HashSet<Guid> ValidAnsweredQuestions = new HashSet<Guid>();
        protected HashSet<Guid> InvalidAnsweredQuestions = new HashSet<Guid>();


        protected abstract IEnumerable<Action<T[]>> ConditionExpressions { get; }

        protected AbstractConditionalLevel(decimal[] rosterVector, Identity[] rosterKey)
        {
            this.RosterVector = rosterVector;
            this.RosterKey = rosterKey;
        }

        private State RunConditionExpression(Func<T[], bool> expression, T[] rosters)
        {
            try
            {
                return expression(rosters) ? State.Enabled : State.Disabled;
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

                //var questionState = this.enablementStatus.FirstOrDefault(x => x.ItemId == id);

                if (this.EnablementStates.ContainsKey(id))
                {
                    //delete
                    //questionState.State = State.Disabled;

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

        protected Action<T[]> Verifier(Func<T[], bool> isEnabled, Guid questionId, ConditionalState questionState)
        {
            return rosters =>
            {
                if (questionState.State == State.Disabled)
                    return;

                questionState.State = this.RunConditionExpression(isEnabled, rosters);
                if (questionState.State == State.Disabled)
                {
                    this.DisableAllDependentQuestions(questionId);
                }
            };
        }

        public void EvaluateConditions(T[] roters, List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled,
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            foreach (var state in this.EnablementStates.Values)
            {
                state.PreviousState = state.State;
                state.State = State.Unknown;
            }

            foreach (Action<T[]> verifier in this.ConditionExpressions)
            {
                verifier(roters);
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
    }

    public abstract class AbstractRosterLevel<T> : AbstractConditionalLevel<T>, IValidatableRoster where T : IValidatable
    {
        protected AbstractRosterLevel(decimal[] rosterVector, Identity[] rosterKey) : base(rosterVector, rosterKey) {}

        protected Dictionary<Identity, Func<T[], bool>> validationExpressions = new Dictionary<Identity, Func<T[], bool>>();

        public abstract void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid);

        public abstract void RunConditions(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeEnabled,
            List<Identity> questionsToBeDisabled, List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);
    }

}
