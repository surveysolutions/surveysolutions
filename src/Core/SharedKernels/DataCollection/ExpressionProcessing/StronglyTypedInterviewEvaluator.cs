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
            var questionnaireLevel = new QuestionnaireLevel(Util.EmptyRosterVector, questionnaireIdentityKey, this.GetRosterInstances);
            this.InterviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
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

            var parent = this.InterviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            if (rosterId == IdOf.hhMember || rosterId == IdOf.jobActivity)
            {
                var parentHolder = parent as QuestionnaireLevel;
                var rosterLevel = new HhMember_type(rosterVector, rosterIdentityKey, parentHolder, this.GetRosterInstances);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                this.SetSiblings(rosterScopeIds, rosterStringKey);
            }

            if (rosterId == IdOf.foodConsumption)
            {
                var parentHolder = parent as HhMember_type;
                var rosterLevel = new FoodConsumption_type(rosterVector, rosterIdentityKey, parentHolder, this.GetRosterInstances);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                this.SetSiblings(rosterScopeIds, rosterStringKey);
            }

            if (rosterId == IdOf.fixedId)
            {
                var parentHolder = parent as QuestionnaireLevel;
                var rosterLevel = new Education_type(rosterVector, rosterIdentityKey, parentHolder, this.GetRosterInstances);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                this.SetSiblings(rosterScopeIds, rosterStringKey);
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
                (targetLevel as HhMember_type).age = answer;
            }

            if (questionId == IdOf.times_per_week)
            {
                (targetLevel as FoodConsumption_type).times_per_week = answer;
            }
        }

        public override void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.price_for_food)
            {
                (targetLevel as FoodConsumption_type).price_for_food = answer;
            }
        }

        public override void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.date)
            {
                (targetLevel as HhMember_type).date = answer;
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
                (targetLevel as HhMember_type).name = answer;
            }

            if (questionId == IdOf.job_title)
            {
                (targetLevel as HhMember_type).job_title = answer;
            }

            if (questionId == IdOf.person_id)
            {
                (targetLevel as HhMember_type).person_id = answer;
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
                (targetLevel as HhMember_type).sex = answer;
            }

            if (questionId == IdOf.role)
            {
                (targetLevel as HhMember_type).role = answer;
            }

            if (questionId == IdOf.has_job)
            {
                (targetLevel as HhMember_type).has_job = answer;
            }

            if (questionId == IdOf.marital_status)
            {
                (targetLevel as HhMember_type).marital_status = answer;
            }

            if (questionId == IdOf.edu_visit)
            {
                (targetLevel as QuestionnaireLevel).edu_visit = answer;
            }

            if (questionId == IdOf.edu)
            {
                (targetLevel as Education_type).edu = answer;
            }
        }

        public override void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.food)
            {
                (targetLevel as HhMember_type).food = answer;
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
                (targetLevel as HhMember_type).best_job_owner = selectedPropagationVector;
            }
        }

        public override void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.married_with)
            {
                (targetLevel as HhMember_type).married_with = answer;
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
    }
}

// ReSharper restore InconsistentNaming

