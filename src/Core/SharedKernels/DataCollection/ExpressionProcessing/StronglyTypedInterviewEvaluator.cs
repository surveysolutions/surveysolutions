using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public class StronglyTypedInterviewEvaluator : AbstractInterviewExpressionState 
    {
        public StronglyTypedInterviewEvaluator()
        {
            var questionnaireLevelScope = new[] { IdOf.questionnaire };
            var questionnaireIdentityKey = Util.GetRosterKey(questionnaireLevelScope, Util.EmptyRosterVector);
            var questionnaireLevel = new QuestionnaireLevel(Util.EmptyRosterVector, questionnaireIdentityKey);
            this.InterviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
            this.SiblingRosters.Add("", new List<string>());
        }

        public StronglyTypedInterviewEvaluator(Dictionary<string, IValidatable> interviewScopes, Dictionary<string, List<string>> siblingRosters)
        {
            InterviewScopes = interviewScopes;
            SiblingRosters = siblingRosters;
        }

        public override void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            Guid[] rosterScopeIds = IdOf.rostersIdToScopeMap[rosterId];
            var rosterIdentityKey = Util.GetRosterKey(rosterScopeIds, rosterVector);
            string rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            if (this.InterviewScopes.ContainsKey(rosterStringKey))
            {
                return;
            }

            decimal[] parentRosterVector = outerRosterVector;

            var rosterParentIdentityKey = parentRosterVector.Length == 0
                ? Util.GetRosterKey(new[] { IdOf.questionnaire }, new decimal[0])
                : Util.GetRosterKey(rosterScopeIds.Shrink(), parentRosterVector);

            var siblingsKey = Util.GetSiblingsKey(rosterScopeIds);

            var parent = this.InterviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            var wasRosterAdded = false;
            if (rosterId == IdOf.hhMember || rosterId == IdOf.jobActivity)
            {
                var rosterLevel = new HhMember(rosterVector, rosterIdentityKey, parent as QuestionnaireLevel);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                wasRosterAdded = true;
            }

            if (rosterId == IdOf.foodConsumption)
            {
                var rosterLevel = new FoodConsumption(rosterVector, rosterIdentityKey, parent as HhMember);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                wasRosterAdded = true;
            }

            if (wasRosterAdded)
            {
                if (!this.SiblingRosters.ContainsKey(siblingsKey))
                {
                    this.SiblingRosters.Add(siblingsKey, new List<string>());
                }
                this.SiblingRosters[siblingsKey].Add(rosterStringKey);
            }
        }

        public override void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);
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

        public override void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, long answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
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

        public override void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.price_for_food)
            {
                (targetLevel as FoodConsumption).price_for_food = answer;
            }
        }

        public override void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.date)
            {
                (targetLevel as HhMember).date = answer;
            }
        }

        public override void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
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
        
        public override void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public override void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
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

        public override void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.food)
            {
                (targetLevel as HhMember).food = answer;
            }
        }

        public override void UpdateGeoLocationAnswer(Guid questionId, decimal[] rosterVector, double latitude, double longitude)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public override void UpdateTextListAnswer(Guid questionId, decimal[] rosterVector, Tuple<decimal, string>[] answers)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public override void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] selectedPropagationVector)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.best_job_owner)
            {
                (targetLevel as HhMember).best_job_owner = selectedPropagationVector;
            }
        }

        public override void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.married_with)
            {
                (targetLevel as HhMember).married_with = answer;
            }
        }

        public override IInterviewExpressionState Clone()
        {
            var newScopes = this.InterviewScopes.ToDictionary(interviewScope => interviewScope.Key, interviewScope => interviewScope.Value.CopyMembers());
            var newSiblingRosters = this.SiblingRosters
                .ToDictionary(
                    interviewScope => interviewScope.Key,
                    interviewScope => new List<string>(interviewScope.Value));

            //set parents
            foreach (var interviewScope in this.InterviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[Util.GetRosterStringKey(parent.GetRosterKey())]);
            }

            return new StronglyTypedInterviewEvaluator(newScopes, newSiblingRosters);
        }

        public override void ProcessConditionExpressions(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, 
            List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            foreach (var interviewScopeKvp in this.InterviewScopes)
            {
                if (interviewScopeKvp.Value is IValidatableRoster)
                {
                    var roster = interviewScopeKvp.Value as IValidatableRoster;

                    var siblingsKey = Util.GetSiblingsKey(interviewScopeKvp.Value.GetRosterKey().Select(x => x.Id).ToArray());

                    var siblingRosters = this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x]);

                    roster.RunConditions(siblingRosters, questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled, groupsToBeDisabled);
                }

                else if (interviewScopeKvp.Value is QuestionnaireLevel)
                {
                    (interviewScopeKvp.Value as QuestionnaireLevel).RunConditions(questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled, groupsToBeDisabled);
                }
            }
        }

        public override void ProcessValidationExpressions(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var interviewScopeKvp in this.InterviewScopes)
            {
                if (interviewScopeKvp.Value is IValidatableRoster)
                {
                    var roster = interviewScopeKvp.Value as IValidatableRoster;
                    var siblingsKey = Util.GetSiblingsKey(interviewScopeKvp.Value.GetRosterKey().Select(x => x.Id).ToArray());

                    var siblingRosters = this.SiblingRosters[siblingsKey].Select(x => this.InterviewScopes[x]);

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

