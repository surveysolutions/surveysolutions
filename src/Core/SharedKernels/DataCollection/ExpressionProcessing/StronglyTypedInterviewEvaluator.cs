using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public class StronglyTypedInterviewEvaluator : IInterviewExpressionState 
    {
        public readonly Dictionary<string, IValidatable> interviewScopes = new Dictionary<string, IValidatable>();

        public StronglyTypedInterviewEvaluator()
        {
            
            var questionnaireIdentityKey = GetRosterKey(new[] { IdOf.questionnaire }, Util.EmptyRosterVector);

            var questionnaireLevel = new QuestionnaireLevel(Util.EmptyRosterVector, questionnaireIdentityKey);

            this.interviewScopes.Add(GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
        }

        public StronglyTypedInterviewEvaluator(Dictionary<string, IValidatable> interviewScopes)
        {
            this.interviewScopes = interviewScopes;
        }

        public static string GetRosterStringKey(Identity[] scopeIds)
        {
            return string.Join("$", scopeIds.Select(ConversionHelper.ConvertIdentityToString));
        }

        public static Identity[] GetRosterKey(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            return rosterScopeIds.Select(x => new Identity(x, rosterVector)).ToArray();
        }

        public void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);

            var rosterIdentityKey = GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);

            if (this.interviewScopes.ContainsKey(GetRosterStringKey(rosterIdentityKey)))
            {
                return;
            }

            decimal[] parentRosterVector = outerRosterVector;

            var rosterParentIdentityKey = parentRosterVector.Length == 0
                ? GetRosterKey(new[] { IdOf.questionnaire }, new decimal[0])
                : GetRosterKey(IdOf.rostersIdToScopeMap[rosterId].Shrink(), parentRosterVector);

            var parent = this.interviewScopes[GetRosterStringKey(rosterParentIdentityKey)];

            if (rosterId == IdOf.hhMember || rosterId == IdOf.jobActivity)
            {
                var rosterLevel = new HhMember(rosterVector, rosterIdentityKey, parent as QuestionnaireLevel);
                this.interviewScopes.Add(GetRosterStringKey(rosterIdentityKey), rosterLevel);
            }

            if (rosterId == IdOf.foodConsumption)
            {
                var rosterLevel = new FoodConsumption(rosterVector, rosterIdentityKey, parent as HhMember);
                this.interviewScopes.Add(GetRosterStringKey(rosterIdentityKey), rosterLevel);
            }
        }

        public void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);
            var dependentRosters = interviewScopes.Keys.Where(x => x.StartsWith(GetRosterStringKey((rosterIdentityKey)))).ToArray();
            foreach (var rosterKey in dependentRosters)
            {
                this.interviewScopes.Remove(rosterKey);
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

            var rosterKey = GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var rosterStringKey = GetRosterStringKey(rosterKey);
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

        public void ProcessValidationExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var interviewScopeKvp in this.interviewScopes)
            {
                if (interviewScopeKvp.Value is AbstractRosterLevel)
                {
                    var interviewScope = interviewScopeKvp.Value as AbstractRosterLevel;

                    var parentRosterVector = interviewScopeKvp.Value.GetRosterKey().Shrink();

                    var parentRosterVectorLength = parentRosterVector.Length;
                    var rosters = this.interviewScopes.Where(x
                        => x.Key.StartsWith(GetRosterStringKey(parentRosterVector))
                            && x.Key.Length == parentRosterVectorLength + 1)
                        .Select(x => x.Value);
                    interviewScope.Validate(rosters, questionsToBeValid, questionsToBeInvalid);
                }

                if (interviewScopeKvp.Value is QuestionnaireLevel)
                {
                    interviewScopeKvp.Value.Validate(questionsToBeValid, questionsToBeInvalid);
                }
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

            //set parents
            foreach (var interviewScope in this.interviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[GetRosterStringKey(parent.GetRosterKey())]);
            }

            return new StronglyTypedInterviewEvaluator(newScopes);

        }
    }
}

// ReSharper restore InconsistentNaming

