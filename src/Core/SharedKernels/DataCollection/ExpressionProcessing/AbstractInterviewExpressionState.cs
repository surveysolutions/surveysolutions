using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public abstract class AbstractInterviewExpressionState : IInterviewExpressionState
    {
        public Dictionary<string, IValidatable> InterviewScopes = new Dictionary<string, IValidatable>();
        public Dictionary<string, List<string>> SiblingRosters = new Dictionary<string, List<string>>();

        public abstract void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex);
        public abstract void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId);

        public abstract void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, long answer);
        public abstract void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer);
        public abstract void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer);
        public abstract void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer);
        public abstract void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer);
        public abstract void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer);
        public abstract void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer);
        public abstract void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude);
        public abstract void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers);
        public abstract void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector);
        public abstract void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors);

        public abstract IInterviewExpressionState Clone();

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            foreach (var identity in invalidQuestions)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);
                if (targetLevel == null) return;

                targetLevel.DeclareAnswerInvalid(identity.Id);
            }
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            foreach (var identity in validQuestions)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);
                if (targetLevel == null) return;

                targetLevel.DeclareAnswerValid(identity.Id);
            }
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            foreach (var identity in groupsToDisable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);
                if (targetLevel == null) return;

                targetLevel.DisableGroup(identity.Id);
            }
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            foreach (var identity in groupsToEnable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);
                if (targetLevel == null) return;

                targetLevel.EnableGroup(identity.Id);
            }
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            foreach (var identity in questionsToDisable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);
                if (targetLevel == null) return;

                targetLevel.DisableQuestion(identity.Id);
            }
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            foreach (var identity in questionsToEnable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);
                if (targetLevel == null) return;

                targetLevel.EnableQuestion(identity.Id);
            }
        }
        
        protected IValidatable GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
        {
            if (!IdOf.parentsMap.ContainsKey(questionId))
                return null;

            var rosterKey = Util.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterKey);
            return this.InterviewScopes.ContainsKey(rosterStringKey) ? this.InterviewScopes[rosterStringKey] : null;
        }

        public void ProcessValidationExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var interviewScopeKvp in this.InterviewScopes)
            {
                interviewScopeKvp.Value.CalculateValidationChanges(questionsToBeValid, questionsToBeInvalid);
            }
        }

        public void ProcessConditionExpressions(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, 
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            foreach (var interviewScopeKvp in this.InterviewScopes)
            {
                interviewScopeKvp.Value.CalculateConditionChanges(questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled, groupsToBeDisabled);
            }
        }

        public IEnumerable<IValidatable> GetRosterInstances(Identity[] rostrerKey)
        {
            var siblingsKey = Util.GetSiblingsKey(rostrerKey.Select(x => x.Id).ToArray());

            var siblingRosters = this.SiblingRosters.ContainsKey(siblingsKey) ?
                this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x])
                : null;

            return siblingRosters;

        }

    }
}
