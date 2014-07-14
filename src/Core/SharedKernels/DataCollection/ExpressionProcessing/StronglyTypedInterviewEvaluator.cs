using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public class StronglyTypedInterviewEvaluator : IInterviewExpressionState 
    {
        public readonly Dictionary<string, IValidatable> interviewScopes = new Dictionary<string, IValidatable>();
        public readonly Dictionary<string, List<string>> siblingRosters = new Dictionary<string, List<string>>();

        public StronglyTypedInterviewEvaluator()
        {
            var questionnaireLevelScope = new[] { IdOf.questionnaire };

            var questionnaireIdentityKey = Util.GetRosterKey(questionnaireLevelScope, Util.EmptyRosterVector);

            var questionnaireLevel = new QuestionnaireLevel(Util.EmptyRosterVector, questionnaireIdentityKey);

            this.interviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);

            this.siblingRosters.Add("", new List<string>());
        }

        public StronglyTypedInterviewEvaluator(Dictionary<string, IValidatable> interviewScopes, Dictionary<string, List<string>> siblingRosters)
        {
            this.interviewScopes = interviewScopes;
            this.siblingRosters = siblingRosters;
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);

            Guid[] rosterScopeIds = IdOf.rostersIdToScopeMap[rosterId];

            var rosterIdentityKey = Util.GetRosterKey(rosterScopeIds, rosterVector);

            string rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            if (this.interviewScopes.ContainsKey(rosterStringKey))
            {
                return;
            }

            decimal[] parentRosterVector = outerRosterVector;

            var rosterParentIdentityKey = parentRosterVector.Length == 0
                ? Util.GetRosterKey(new[] { IdOf.questionnaire }, new decimal[0])
                : Util.GetRosterKey(rosterScopeIds.Shrink(), parentRosterVector);

            var siblingsKey = Util.GetSiblingsKey(rosterScopeIds);

            var parent = this.interviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            var wasRosterAdded = false;
            if (rosterId == IdOf.hhMember || rosterId == IdOf.jobActivity)
            {
                var rosterLevel = new HhMember(rosterVector, rosterIdentityKey, parent as QuestionnaireLevel);
                this.interviewScopes.Add(rosterStringKey, rosterLevel);
                wasRosterAdded = true;
            }

            if (rosterId == IdOf.foodConsumption)
            {
                var rosterLevel = new FoodConsumption(rosterVector, rosterIdentityKey, parent as HhMember);
                this.interviewScopes.Add(rosterStringKey, rosterLevel);
                wasRosterAdded = true;
            }

            if (wasRosterAdded)
            {
                if (!this.siblingRosters.ContainsKey(siblingsKey))
                {
                    this.siblingRosters.Add(siblingsKey, new List<string>());
                }
                this.siblingRosters[siblingsKey].Add(rosterStringKey);
            }
        }

        public void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);
            var dependentRosters = interviewScopes.Keys.Where(x => x.StartsWith(Util.GetRosterStringKey((rosterIdentityKey)))).ToArray();
            foreach (var rosterKey in dependentRosters)
            {
                this.interviewScopes.Remove(rosterKey);
                foreach (var siblings in siblingRosters.Values)
                {
                    siblings.Remove(rosterKey);
                }
            }
        }

        public void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, long answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.persons_count)
            {
                (targetLevel as QuestionnaireLevel).persons_count = answer;
            }

            if (questionId == IdOf.age)
            {
                (targetLevel as HhMember).age = answer;
            }

            if (questionId == IdOf.times_per_week)
            {
                (targetLevel as FoodConsumption).times_per_week = answer;
            }
        }

        public void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.price_for_food)
            {
                (targetLevel as FoodConsumption).price_for_food = answer;
            }
        }

        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.date)
            {
                (targetLevel as HhMember).date = answer;
            }
        }

        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.id)
            {
                (targetLevel as QuestionnaireLevel).id = answer;
            }

            if (questionId == IdOf.name)
            {
                (targetLevel as HhMember).name = answer;
            }

            if (questionId == IdOf.job_title)
            {
                (targetLevel as HhMember).job_title = answer;
            }

            if (questionId == IdOf.person_id)
            {
                (targetLevel as HhMember).person_id = answer;
            }
        }

        private IValidatable GetRosterToUpdateAnswer(Guid questionId, decimal[] rosterVector)
        {
            if (!IdOf.parentsMap.ContainsKey(questionId))
                return null;

            var rosterKey = Util.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterKey);
            return this.interviewScopes.ContainsKey(rosterStringKey) ? this.interviewScopes[rosterStringKey] : null;
        }

        public void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.sex)
            {
                (targetLevel as HhMember).sex = answer;
            }

            if (questionId == IdOf.role)
            {
                (targetLevel as HhMember).role = answer;
            }

            if (questionId == IdOf.has_job)
            {
                (targetLevel as HhMember).has_job = answer;
            }

            if (questionId == IdOf.marital_status)
            {
                (targetLevel as HhMember).marital_status = answer;
            }
        }

        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.food)
            {
                (targetLevel as HhMember).food = answer;
            }
        }

        public void UpdateGeoLocationAnswer(Guid questionId, decimal[] rosterVector, double latitude, double longitude)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public void UpdateTextListAnswer(Guid questionId, decimal[] rosterVector, Tuple<decimal, string>[] answers)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] selectedPropagationVector)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.best_job_owner)
            {
                (targetLevel as HhMember).best_job_owner = selectedPropagationVector;
            }
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] answer)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.married_with)
            {
                (targetLevel as HhMember).married_with = answer;
            }
        }

        public void DisableGroups(IEnumerable<Identity> groupsToDisable) { }

        public void EnableGroups(IEnumerable<Identity> groupsToEnable) { }

        public void DisableQuestions(IEnumerable<Identity> questionsToDisable) { }

        public void EnableQuestions(IEnumerable<Identity> questionsToEnable) { }

        public IInterviewExpressionState Clone()
        {
            var newScopes = this.interviewScopes.ToDictionary(interviewScope => interviewScope.Key, interviewScope => interviewScope.Value.CopyMembers());
            var newSiblingRosters = this.siblingRosters
                .ToDictionary(
                    interviewScope => interviewScope.Key,
                    interviewScope => new List<string>(interviewScope.Value));

            //set parents
            foreach (var interviewScope in this.interviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[Util.GetRosterStringKey(parent.GetRosterKey())]);
            }

            return new StronglyTypedInterviewEvaluator(newScopes, newSiblingRosters);

        }

        public void ProcessValidationExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var interviewScopeKvp in this.interviewScopes)
            {
                if (interviewScopeKvp.Value is IValidatableRoster)
                {
                    var roster = interviewScopeKvp.Value as IValidatableRoster;
                    var siblingsKey = Util.GetSiblingsKey(interviewScopeKvp.Value.GetRosterKey().Select(x => x.Id).ToArray());

                    var siblingRosters = this.siblingRosters[siblingsKey].Select(x => this.interviewScopes[x]);

                    roster.Validate(siblingRosters, questionsToBeValid, questionsToBeInvalid);
                }

                if (interviewScopeKvp.Value is QuestionnaireLevel)
                {
                    (interviewScopeKvp.Value as QuestionnaireLevel).Validate(questionsToBeValid, questionsToBeInvalid);
                }
            }
        }
    }
}

// ReSharper restore InconsistentNaming

