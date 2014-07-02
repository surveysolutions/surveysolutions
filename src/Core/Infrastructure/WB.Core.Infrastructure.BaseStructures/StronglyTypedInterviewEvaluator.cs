using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace WB.Core.Infrastructure.BaseStructures
{
    public class Identity
    {
        // should be shared
        public Guid Id { get; private set; }
        public decimal[] RosterVector { get; private set; }

        public Identity(Guid id, decimal[] rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }
    }

    public interface IValidatable
    {
        void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }

    public interface IValidatableRoster
    {
        void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }

    public class QuestionnaireLevel : IValidatable
    {
        private readonly decimal[] rosterVector;

        public QuestionnaireLevel(decimal[] rosterVector)
        {
            this.rosterVector = rosterVector;
        }

        public string id;
        public int? persons_count;

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid) { }
       
    }

    public class HhMember : IValidatable, IValidatableRoster
    {
        private readonly decimal[] rosterVector;

        public HhMember(decimal[] rosterVector, QuestionnaireLevel parent)
        {
            this.rosterVector = rosterVector;
            this.parent = parent;
        }

        private QuestionnaireLevel parent;

        public string id { get { return parent.id; } }
        public int? persons_count { get { return parent.persons_count; } }

        public string name { get; set; }
        public int? age { get; set; }
        public DateTime? date { get; set; }
        public decimal? sex { get; set; }
        public decimal? role { get; set; }
        public decimal[] food { get; set; }
        public decimal? has_job { get; set; }
        public string job_title { get; set; }
        public decimal? best_job_owner { get; set; }

        private bool age_Validation()
        {
            return age >= 3;
        }

        private void Validate(IEnumerable<HhMember> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            //if (age_Validation().CanBeProcessed)
            {
                var ageIdentity = new Identity(Id.ageId, rosterVector);
                if (age_Validation())
                    questionsToBeValid.Add(ageIdentity);
                else
                    questionsToBeInvalid.Add(ageIdentity);
            }
        }

        public void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(Enumerable.Empty<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }
    }

  
    public class FoodConsumption : IValidatable
    {
        private readonly decimal[] rosterVector;
        public FoodConsumption(decimal[] rosterVector, HhMember parent)
        {
            this.rosterVector = rosterVector;
            this.parent = parent;
        }

        private HhMember parent;

        public string id { get { return parent.id; } }
        public int? persons_count { get { return parent.persons_count; } }
        public string name { get { return parent.name; } }
        public int? age { get { return parent.age; } }
        public DateTime? date { get { return parent.date; } }
        public decimal? sex { get { return parent.sex; } }
        public decimal? role { get { return parent.role; } }
        public decimal[] food { get { return parent.food; } }
        public decimal? has_job { get { return parent.has_job; } }
        public string job_title { get { return parent.job_title; } }
        public decimal? best_job_owner { get { return parent.best_job_owner; } }

        public int times_per_week;
        public decimal price_for_food;

        private void Validate(IEnumerable<FoodConsumption> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {

        }

        public void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<FoodConsumption>(), questionsToBeValid, questionsToBeInvalid);
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(Enumerable.Empty<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }
    }

      public static class Id {
          public static readonly Guid idId = Guid.Parse("45ba5cc2-9bd7-4c76-8f13-94432405917a");
          public static readonly Guid persons_countId = Guid.Parse("4139db8c-d3cc-49a7-a86f-0e1b5fd31edd");
          public static readonly Guid nameId = Guid.Parse("03430fc9-3ab3-4e65-8a87-5af620b8e500");
          public static readonly Guid ageId = Guid.Parse("552c411f-e0ed-4883-bccd-138e1893a64d");
          public static readonly Guid dateId = Guid.Parse("6d7c3c4b-cbc5-4a13-a14f-7a882b54cf61");
          public static readonly Guid sexId = Guid.Parse("51b9d76b-d9a5-426c-842e-33a930cf155e");
          public static readonly Guid roleId = Guid.Parse("7e630572-6f23-428b-8d75-ceda05c1e969");
          public static readonly Guid foodId = Guid.Parse("072be626-db13-4cd5-aeff-013e56e65932");
          public static readonly Guid times_per_weekId = Guid.Parse("e0fe6bf7-ea6b-4897-b526-e0e96d7d0b0b");
          public static readonly Guid price_for_foodId = Guid.Parse("571919ee-ff41-498d-9fed-c564b4ef80d6");
          public static readonly Guid has_jobId = Guid.Parse("9aedd04e-b370-4281-b20c-aaa85988a4c9");
          public static readonly Guid job_titleId = Guid.Parse("f93a4462-5b3d-4782-8e6a-af9af02db3a3");
          public static readonly Guid best_job_ownerId = Guid.Parse("246ad6a8-cd73-4d0c-b5fc-1aa21c101ffe");

          public static readonly Guid[] hhMemberScopeIds = new []{ persons_countId};
          public static readonly Guid[] foodConsumptionIds = new[] { foodId };
      }

    public class StronglyTypedInterviewEvaluator : IInterviewEvaluator
    {
        private readonly Dictionary<decimal[], IValidatable> interviewScopes = new Dictionary<decimal[], IValidatable>();

        public StronglyTypedInterviewEvaluator()
        {
            var emptyRosterVector = new decimal[0];
            var questionnaireLevel = new QuestionnaireLevel(emptyRosterVector);
            interviewScopes.Add(emptyRosterVector, questionnaireLevel);
        }

        public void AddRoster(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            if (rosterScopeIds.SequenceEqual( Id.hhMemberScopeIds))
            {
                var parent = interviewScopes[rosterVector.Take(rosterVector.Length - 1).ToArray()] as QuestionnaireLevel;
                var rosterLevel = new HhMember(rosterVector, parent);
                interviewScopes.Add(rosterVector, rosterLevel);
            }

            if (rosterScopeIds.SequenceEqual(Id.foodConsumptionIds))
            {
                var parent = interviewScopes[rosterVector.Take(rosterVector.Length - 1).ToArray()] as HhMember;
                var rosterLevel = new FoodConsumption(rosterVector, parent);
                interviewScopes.Add(rosterVector, rosterLevel);
            }
        }

        public void RemoveRoster(decimal[] rosterVector)
        {
            var dependentRosters = interviewScopes.Keys.Where(x => x.Take(rosterVector.Length).SequenceEqual(rosterVector));
            foreach (var rosterVectorKey in dependentRosters)
            {
                interviewScopes.Remove(rosterVectorKey);
            }
        }
        public void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, int answer)
        {
            var targetLevel = interviewScopes.ContainsKey(rosterVector) ? interviewScopes[rosterVector] : null;
            if (targetLevel == null) return;

            if (questionId == Id.persons_countId)
            {
                (targetLevel as QuestionnaireLevel).persons_count = answer;
            }

            if (questionId == Id.ageId)
            {
                (targetLevel as HhMember).age = answer;
            }

            if (questionId == Id.times_per_weekId)
            {
                (targetLevel as FoodConsumption).times_per_week = answer;
            }
        }
        public void UpdateDecimalAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = interviewScopes.ContainsKey(rosterVector) ? interviewScopes[rosterVector] : null;
            if (targetLevel == null) return;

            if (questionId == Id.price_for_foodId)
            {
                (targetLevel as FoodConsumption).price_for_food = answer;
            }
        }
        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            var targetLevel = interviewScopes.ContainsKey(rosterVector) ? interviewScopes[rosterVector] : null;
            if (targetLevel == null) return;

            if (questionId == Id.dateId)
            {
                (targetLevel as HhMember).date = answer;
            }
        }
        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = interviewScopes.ContainsKey(rosterVector) ? interviewScopes[rosterVector] : null;
            if (targetLevel == null) return;

            if (questionId == Id.idId)
            {
                (targetLevel as QuestionnaireLevel).id = answer;
            }

            if (questionId == Id.nameId)
            {
                (targetLevel as HhMember).name = answer;
            }

            if (questionId == Id.job_titleId)
            {
                (targetLevel as HhMember).job_title = answer;
            }
        }
        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var targetLevel = interviewScopes.ContainsKey(rosterVector) ? interviewScopes[rosterVector] : null;
            if (targetLevel == null) return;

            if (questionId == Id.sexId)
            {
                (targetLevel as HhMember).sex = answer;
            }

            if (questionId == Id.roleId)
            {
                (targetLevel as HhMember).role = answer;
            }

            if (questionId == Id.has_jobId)
            {
                (targetLevel as HhMember).has_job = answer;
            }

            if (questionId == Id.best_job_ownerId)
            {
                (targetLevel as HhMember).best_job_owner = answer;
            }
        }
        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = interviewScopes.ContainsKey(rosterVector) ? interviewScopes[rosterVector] : null;
            if (targetLevel == null) return;

            if (questionId == Id.foodId)
            {
                (targetLevel as HhMember).food = answer;
            }
        }

        public int Test()
        {
            var questionsToBeValid = new List<Identity>();
            var questionsToBeInvalid = new List<Identity>();

            foreach (var interviewScope in interviewScopes.Values)
            {
                interviewScope.Validate(questionsToBeValid, questionsToBeInvalid);
            }

            return 8;
        }
    }
}
// ReSharper restore InconsistentNaming

