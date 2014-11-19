using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public abstract class AbstractInterviewExpressionState : IInterviewExpressionState
    {
        public Dictionary<string, IExpressionExecutable> InterviewScopes = new Dictionary<string, IExpressionExecutable>();
        public Dictionary<string, List<string>> SiblingRosters = new Dictionary<string, List<string>>();

        public virtual void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!this.HasParentMapSuchRostre(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(this.GetRosterParentScopeMap(rosterId), rosterVector);

            var dependentRosters = this.InterviewScopes.Keys.Where(x => x.StartsWith(Util.GetRosterStringKey((rosterIdentityKey)))).ToArray();

            foreach (var rosterKey in dependentRosters)
            {
                this.InterviewScopes.Remove(rosterKey);
                foreach (var siblings in this.SiblingRosters.Values)
                {
                    siblings.Remove(rosterKey);
                }
            }
        }

        public virtual void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            if (!this.HasParentMapSuchRostre(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            Guid[] rosterScopeIds = this.GetRosterParentScopeMap(rosterId); 
            var rosterIdentityKey = Util.GetRosterKey(rosterScopeIds, rosterVector);
            string rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            if (this.InterviewScopes.ContainsKey(rosterStringKey))
            {
                return;
            }

            var rosterParentIdentityKey = outerRosterVector.Length == 0
                ? Util.GetRosterKey(new[] { GetQuestionnaireId()  }, new decimal[0])
                : Util.GetRosterKey(rosterScopeIds.Shrink(), outerRosterVector);

            var parent = this.InterviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            var rosterLevel = parent.CreateChildRosterInstance(rosterId, rosterVector, rosterIdentityKey);

            this.InterviewScopes.Add(rosterStringKey, rosterLevel);
            this.SetSiblings(rosterIdentityKey, rosterStringKey);
        }

        public abstract bool HasParentMapSuchRostre(Guid rosterId);

        public abstract Guid GetQuestionnaireId();

        public abstract Guid[] GetRosterParentScopeMap(Guid rosterId);
       

        public virtual void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateNumericIntegerAnswer(questionId, answer);
        }

        public virtual void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateNumericRealAnswer(questionId, answer);
        }

        public virtual void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateDateTimeAnswer(questionId, answer);
        }

        public virtual void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateMediaAnswer(questionId, answer);
        }

        public virtual void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateTextAnswer(questionId, answer);
        }

        public virtual void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateQrBarcodeAnswer(questionId, answer);
        }

        public virtual void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateSingleOptionAnswer(questionId, answer);
        }

        public virtual void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateMultiOptionAnswer(questionId, answer);
        }

        public virtual void UpdateGeoLocationAnswer(Guid questionId, decimal[] rosterVector, double latitude, double longitude, double accuracy, double altitude)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateGeoLocationAnswer(questionId, latitude, longitude, accuracy, altitude);
        }

        public virtual void UpdateTextListAnswer(Guid questionId, decimal[] rosterVector, Tuple<decimal, string>[] answers)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateTextListAnswer(questionId, answers);
        }

        public virtual void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] selectedPropagationVector)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateLinkedSingleOptionAnswer(questionId, selectedPropagationVector);
        }

        public virtual void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateLinkedMultiOptionAnswer(questionId, answer);
        }


        public abstract Dictionary<Guid, Guid[]> GetParentsMap();

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

        protected IExpressionExecutable GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
        {
            var parentsMap = this.GetParentsMap();
            if (!parentsMap.ContainsKey(questionId))
                return null;

            var rosterKey = Util.GetRosterKey(parentsMap[questionId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterKey);
            return this.InterviewScopes.ContainsKey(rosterStringKey) ? this.InterviewScopes[rosterStringKey] : null;
        }

        public ValidityChanges ProcessValidationExpressions()
        {
            var questionsToBeValid = new List<Identity>();
            var questionsToBeInvalid = new List<Identity>();

            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values)
            {
                List<Identity> questionsToBeValidByScope;
                List<Identity> questionsToBeInvalidByScope;

                interviewScopeKvpValue.CalculateValidationChanges(out questionsToBeValidByScope, out questionsToBeInvalidByScope);

                questionsToBeValid.AddRange(questionsToBeValidByScope);
                questionsToBeInvalid.AddRange(questionsToBeInvalidByScope);
            }

            return new ValidityChanges(answersDeclaredValid: questionsToBeValid, answersDeclaredInvalid: questionsToBeInvalid);
        }

        public EnablementChanges ProcessEnablementConditions()
        {
            var questionsToBeEnabled = new List<Identity>();
            var questionsToBeDisabled = new List<Identity>();
            var groupsToBeEnabled = new List<Identity>();
            var groupsToBeDisabled = new List<Identity>();

            //order by scope depth starting from top
            //conditionally lower scope could depend only from upper scope
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values.OrderBy(x => x.GetLevel()))
            {
                List<Identity> questionsToBeEnabledArray;
                List<Identity> questionsToBeDisabledArray;
                List<Identity> groupsToBeEnabledArray;
                List<Identity> groupsToBeDisabledArray;

                interviewScopeKvpValue.CalculateConditionChanges(out questionsToBeEnabledArray, out questionsToBeDisabledArray, out groupsToBeEnabledArray,
                    out groupsToBeDisabledArray);

                questionsToBeEnabled.AddRange(questionsToBeEnabledArray);
                questionsToBeDisabled.AddRange(questionsToBeDisabledArray);
                groupsToBeEnabled.AddRange(groupsToBeEnabledArray);
                groupsToBeDisabled.AddRange(groupsToBeDisabledArray);
            }

            return new EnablementChanges(groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled);
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values.OrderBy(x => x.GetLevel()))
            {
                interviewScopeKvpValue.SaveAllCurrentStatesAsPrevious();
            }
        }

        public IEnumerable<IExpressionExecutable> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
        {
             var siblingsKey = Util.GetSiblingsKey(rosterKey, scopeId);

            var siblingRosters = this.SiblingRosters.ContainsKey(siblingsKey)
                ? this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x])
                : null;

            return siblingRosters;
        }

        protected void SetSiblings(Identity[] rosterKey, string rosterStringKey)
        {
            var siblingsKey = Util.GetSiblingsKey(rosterKey);

            if (!this.SiblingRosters.ContainsKey(siblingsKey))
            {
                this.SiblingRosters.Add(siblingsKey, new List<string>());
            }
            this.SiblingRosters[siblingsKey].Add(rosterStringKey);
        }
    }
}