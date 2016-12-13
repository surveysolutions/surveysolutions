using System;
using System.Collections.Generic;
using System.Linq;


namespace WB.Core.SharedKernels.DataCollection.V4
{
    public abstract class AbstractInterviewExpressionStateV2 : IInterviewExpressionState
    {
        public Dictionary<string, List<string>> SiblingRosters = new Dictionary<string, List<string>>();


        public abstract void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex);

        public abstract void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId);

        
        public abstract Dictionary<Guid, Guid[]> GetParentsMap();

        public abstract IInterviewExpressionState Clone();

        public void DeclareAnswersInvalid(IEnumerable<Identity> invalidQuestions)
        {
            foreach (var identity in invalidQuestions)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);

                targetLevel?.DeclareAnswerInvalid(identity.Id);
            }
        }

        public void DeclareAnswersValid(IEnumerable<Identity> validQuestions)
        {
            foreach (var identity in validQuestions)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);

                targetLevel?.DeclareAnswerValid(identity.Id);
            }
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable)
        {
            foreach (var identity in groupsToDisable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);

                targetLevel?.DisableGroup(identity.Id);
            }
        }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable)
        {
            foreach (var identity in groupsToEnable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);

                targetLevel?.EnableGroup(identity.Id);
            }
        }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable)
        {
            foreach (var identity in questionsToDisable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);

                targetLevel?.DisableQuestion(identity.Id);
            }
        }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable)
        {
            foreach (var identity in questionsToEnable)
            {
                var targetLevel = this.GetRosterByIdAndVector(identity.Id, identity.RosterVector);

                targetLevel?.EnableQuestion(identity.Id);
            }
        }
        
        
        protected void SetSiblings(Identity[] rosterKey, string rosterStringKey, int? sortIndex = null)
        {
            var siblingsKey = Util.GetSiblingsKey(rosterKey);

            if (!this.SiblingRosters.ContainsKey(siblingsKey))
            {
                this.SiblingRosters.Add(siblingsKey, new List<string>());
            }

            if (sortIndex.HasValue && this.SiblingRosters[siblingsKey].Count > sortIndex.Value)
            {
                this.SiblingRosters[siblingsKey].Insert(sortIndex.Value, rosterStringKey);
            }
            else
                this.SiblingRosters[siblingsKey].Add(rosterStringKey);
        }

        public Dictionary<string, IExpressionExecutableV2> InterviewScopes = new Dictionary<string, IExpressionExecutableV2>();

        public IInterviewProperties InterviewProperties { set; get; }

        public AbstractInterviewExpressionStateV2()
        {
        }

        public AbstractInterviewExpressionStateV2(Dictionary<string, IExpressionExecutableV2> interviewScopes, Dictionary<string, List<string>> siblingRosters, IInterviewProperties interviewProperties)
        {
            this.InterviewProperties = interviewProperties.Clone();

            var newScopes = interviewScopes.ToDictionary(interviewScope => interviewScope.Key, interviewScope => interviewScope.Value.CopyMembers(this.GetRosterInstances));

            var newSiblingRosters = siblingRosters
                .ToDictionary(
                    interviewScope => interviewScope.Key,
                    interviewScope => new List<string>(interviewScope.Value));


            foreach (var interviewScope in interviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[Util.GetRosterStringKey(parent.GetRosterKey())]);

                interviewScope.Value.SetInterviewProperties(this.InterviewProperties);
            }

            this.InterviewScopes = newScopes;
            this.SiblingRosters = newSiblingRosters;
        }

        public void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateNumericIntegerAnswer(questionId, answer);
        }

        public void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateNumericRealAnswer(questionId, answer);
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateDateTimeAnswer(questionId, answer);
        }

        public void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateMediaAnswer(questionId, answer);
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateTextAnswer(questionId, answer);
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateQrBarcodeAnswer(questionId, answer);
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateSingleOptionAnswer(questionId, answer);
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateMultiOptionAnswer(questionId, answer);
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] rosterVector, double latitude,
            double longitude, double accuracy, double altitude)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateGeoLocationAnswer(questionId, latitude, longitude, accuracy, altitude);
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] rosterVector,
            Tuple<decimal, string>[] answers)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateTextListAnswer(questionId, answers);
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] rosterVector,
            decimal[] selectedPropagationVector)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateLinkedSingleOptionAnswer(questionId, selectedPropagationVector);
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateLinkedMultiOptionAnswer(questionId, answer);
        }

        public IEnumerable<IExpressionExecutableV2> GetRosterInstances(Identity[] rosterKey, Guid scopeId)
        {
            var siblingsKey = Util.GetSiblingsKey(rosterKey, scopeId);

            var siblingRosters = this.SiblingRosters.ContainsKey(siblingsKey)
                ? this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x])
                : null;

            return siblingRosters;
        }

        protected virtual IExpressionExecutable GetRosterByIdAndVector(Guid questionId, decimal[] rosterVector)
        {
            var parentsMap = this.GetParentsMap();
            if (!parentsMap.ContainsKey(questionId))
                return null;

            var rosterKey = Util.GetRosterKey(parentsMap[questionId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterKey);
            return this.InterviewScopes.ContainsKey(rosterStringKey) ? this.InterviewScopes[rosterStringKey] : null;
        }

        public void SaveAllCurrentStatesAsPrevious()
        {
            foreach (var interviewScopeKvpValue in this.InterviewScopes.Values.OrderBy(x => x.GetLevel()))
            {
                interviewScopeKvpValue.SaveAllCurrentStatesAsPrevious();
            }
        }

        public ValidityChanges ProcessValidationExpressions() => ProcessValidationExpressionsImpl(this.InterviewScopes.Values);

        protected static ValidityChanges ProcessValidationExpressionsImpl(IEnumerable<IExpressionExecutable> interviewScopes)
            => AbstractInterviewExpressionState.ProcessValidationExpressionsImpl(interviewScopes);

        public EnablementChanges ProcessEnablementConditions() => ProcessEnablementConditionsImpl(this.InterviewScopes.Values);

        protected static EnablementChanges ProcessEnablementConditionsImpl(IEnumerable<IExpressionExecutable> interviewScopes)
            => AbstractInterviewExpressionState.ProcessEnablementConditionsImpl(interviewScopes);
    }
}