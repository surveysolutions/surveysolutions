using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace WB.Core.Infrastructure.BaseStructures
{
    public interface IValidatable
    {
        void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }

    public interface IValidatableRoster
    {
        void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }

    public abstract class AbstractRosterLevel : IValidatableRoster
    {
        public readonly decimal[] rosterVector;

        protected AbstractRosterLevel(decimal[] rosterVector)
        {
            this.rosterVector = rosterVector;
        }

        public abstract void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid);
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

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            //Members.Count(m => m.age > 15);
        }
    }

    public class HhMember : AbstractRosterLevel, IValidatable, IValidatableRoster
    {
        public HhMember(decimal[] rosterVector, QuestionnaireLevel parent)
            : base(rosterVector)
        {
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

        private bool age_IsValid()
        {
            // person should not be too young and too old
            return age >= 0 && age < 100;
        }

        private bool food_IsValid()
        {
            // children should not dring alcohol
            return food.Contains(38) && role == 3 && age >= 21;
        }

        private void Validate(IEnumerable<HhMember> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            //if (age_Validation().CanBeProcessed)
            {
                var ageIdentity = new Identity(IdOf.age, rosterVector);
                (this.age_IsValid() ? questionsToBeValid : questionsToBeInvalid).Add(ageIdentity);
            }
            //if (food_IsValid().CanBeProcessed)
            {
                var ageIdentity = new Identity(IdOf.age, rosterVector);
                (this.age_IsValid() ? questionsToBeValid : questionsToBeInvalid).Add(ageIdentity);
            }
        }


        public override void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(Enumerable.Empty<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }
    }

    public class FoodConsumption : AbstractRosterLevel, IValidatable, IValidatableRoster
    {
        public FoodConsumption(decimal[] rosterVector, HhMember parent)
            : base(rosterVector)
        {
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

        public override void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<FoodConsumption>(), questionsToBeValid, questionsToBeInvalid);
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(Enumerable.Empty<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }
    }

    public static class IdOf
    {
        public static readonly Guid id = Guid.Parse("45ba5cc2-9bd7-4c76-8f13-94432405917a");
        public static readonly Guid persons_count = Guid.Parse("4139db8c-d3cc-49a7-a86f-0e1b5fd31edd");
        public static readonly Guid name = Guid.Parse("03430fc9-3ab3-4e65-8a87-5af620b8e500");
        public static readonly Guid age = Guid.Parse("552c411f-e0ed-4883-bccd-138e1893a64d");
        public static readonly Guid date = Guid.Parse("6d7c3c4b-cbc5-4a13-a14f-7a882b54cf61");
        public static readonly Guid sex = Guid.Parse("51b9d76b-d9a5-426c-842e-33a930cf155e");
        public static readonly Guid role = Guid.Parse("7e630572-6f23-428b-8d75-ceda05c1e969");
        public static readonly Guid food = Guid.Parse("072be626-db13-4cd5-aeff-013e56e65932");
        public static readonly Guid times_per_week = Guid.Parse("e0fe6bf7-ea6b-4897-b526-e0e96d7d0b0b");
        public static readonly Guid price_for_food = Guid.Parse("571919ee-ff41-498d-9fed-c564b4ef80d6");
        public static readonly Guid has_job = Guid.Parse("9aedd04e-b370-4281-b20c-aaa85988a4c9");
        public static readonly Guid job_title = Guid.Parse("f93a4462-5b3d-4782-8e6a-af9af02db3a3");
        public static readonly Guid best_job_owner = Guid.Parse("246ad6a8-cd73-4d0c-b5fc-1aa21c101ffe");

        public static readonly Guid[] hhMemberScopeIds = new[] { persons_count };
        public static readonly Guid[] foodConsumptionIds = new[] { persons_count, food };

        public static readonly Guid questionnaire = Guid.Parse("72897e3f-3dc8-4115-81e0-8a9c1cadec2d");
        public static readonly Guid hhMember = Guid.Parse("69b02bcf-0ed1-4b5a-80bb-bd465ab096da");
        public static readonly Guid foodConsumption = Guid.Parse("6df41d95-c785-4452-8303-bed3985d4c20");
        public static readonly Guid jobActivity = Guid.Parse("7556ddf0-457a-4a9b-a021-45ac3bad05a8");

        public static Dictionary<Guid, Guid[]> parentsMap = new Dictionary<Guid, Guid[]>
        {
            { id, new Guid[0] },
            { persons_count, new Guid[0] },
            { name, hhMemberScopeIds },
            { age, hhMemberScopeIds },
            { date, hhMemberScopeIds },
            { sex, hhMemberScopeIds },
            { role, hhMemberScopeIds },
            { food, hhMemberScopeIds },
            { times_per_week, foodConsumptionIds },
            { price_for_food, foodConsumptionIds },
            { has_job, hhMemberScopeIds },
            { job_title, hhMemberScopeIds },
            { best_job_owner, hhMemberScopeIds },
        };

        public static Dictionary<Guid, Guid[]> rostersIdToScopeMap = new Dictionary<Guid, Guid[]>
        {
            {hhMember, hhMemberScopeIds},
            {foodConsumption, foodConsumptionIds},
            {jobActivity, hhMemberScopeIds}
        };
    }

    public class StronglyTypedInterviewEvaluator : IInterviewEvaluator
    {
        private readonly Dictionary<Identity[], IValidatable> interviewScopes = new Dictionary<Identity[], IValidatable>();

        public StronglyTypedInterviewEvaluator()
        {
            var emptyRosterVector = new decimal[0];
            var questionnaireLevel = new QuestionnaireLevel(emptyRosterVector);
            var questionnaireIdentityKey = new[]
            {
                new Identity(IdOf.questionnaire, emptyRosterVector)
            };
            interviewScopes.Add(questionnaireIdentityKey, questionnaireLevel);
        }

        private Identity[] GetRosterKey(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            return rosterScopeIds.Select(x => new Identity(x, rosterVector)).ToArray();
        }

        public void AddRoster(Guid rosterId, decimal[] rosterVector)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] parentRosterVector = rosterVector.Shrink();

            var rosterParentIdentityKey = parentRosterVector.Length == 0 
                ? this.GetRosterKey(new[] { IdOf.questionnaire }, new decimal[0]) 
                : this.GetRosterKey(IdOf.rostersIdToScopeMap[rosterId].Shrink(), parentRosterVector);

            var parent = interviewScopes[rosterParentIdentityKey];
            var rosterIdentityKey = this.GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);

            if (rosterId == IdOf.hhMember || rosterId == IdOf.jobActivity)
            {
                var rosterLevel = new HhMember(rosterVector, parent as QuestionnaireLevel);
                interviewScopes.Add(rosterIdentityKey, rosterLevel);
            }

            if (rosterId == IdOf.foodConsumption)
            {
                var rosterLevel = new FoodConsumption(rosterVector, parent as HhMember);
                interviewScopes.Add(rosterIdentityKey, rosterLevel);
            }
        }

        public void RemoveRoster(Guid rosterId, decimal[] rosterVector)
        {
            var rosterIdentityKey = this.GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);
            var dependentRosters = interviewScopes.Keys.Where(x => x.Take(rosterVector.Length).SequenceEqual(rosterIdentityKey));
            foreach (var rosterVectorKey in dependentRosters)
            {
                interviewScopes.Remove(rosterVectorKey);
            }
        }
        public void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, int answer)
        {
            var rosterKey = this.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var targetLevel = interviewScopes.ContainsKey(rosterKey) ? interviewScopes[rosterKey] : null;
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
            var rosterKey = this.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var targetLevel = interviewScopes.ContainsKey(rosterKey) ? interviewScopes[rosterKey] : null;
            if (targetLevel == null) return;

            if (questionId == IdOf.price_for_food)
            {
                (targetLevel as FoodConsumption).price_for_food = answer;
            }
        }
        public void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            var rosterKey = this.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var targetLevel = interviewScopes.ContainsKey(rosterKey) ? interviewScopes[rosterKey] : null;
            if (targetLevel == null) return;

            if (questionId == IdOf.date)
            {
                (targetLevel as HhMember).date = answer;
            }
        }
        public void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var rosterKey = this.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var targetLevel = interviewScopes.ContainsKey(rosterKey) ? interviewScopes[rosterKey] : null;
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
        }
        public void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            var rosterKey = this.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var targetLevel = interviewScopes.ContainsKey(rosterKey) ? interviewScopes[rosterKey] : null;
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

            if (questionId == IdOf.best_job_owner)
            {
                (targetLevel as HhMember).best_job_owner = answer;
            }
        }
        public void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var rosterKey = this.GetRosterKey(IdOf.parentsMap[questionId], rosterVector);
            var targetLevel = interviewScopes.ContainsKey(rosterKey) ? interviewScopes[rosterKey] : null;
            if (targetLevel == null) return;

            if (questionId == IdOf.food)
            {
                (targetLevel as HhMember).food = answer;
            }
        }

        public int Test()
        {
            var questionsToBeValid = new List<Identity>();
            var questionsToBeInvalid = new List<Identity>();

            foreach (var interviewScopeKvp in interviewScopes)
            {
                if (interviewScopeKvp.Value is AbstractRosterLevel)
                {
                    var interviewScope = interviewScopeKvp.Value as AbstractRosterLevel;

                    var parentRosterVector = interviewScopeKvp.Key.Shrink();

                    var parentRosterVectorLength = parentRosterVector.Length;
                    var rosters = interviewScopes.Where(x
                        => x.Key.Take(parentRosterVectorLength).SequenceEqual(parentRosterVector)
                        && x.Key.Length == parentRosterVectorLength + 1)
                        .Select(x => x.Value);
                    interviewScope.Validate(rosters, questionsToBeValid, questionsToBeInvalid);
                }

                if (interviewScopeKvp.Value is QuestionnaireLevel)
                {
                    interviewScopeKvp.Value.Validate(questionsToBeValid, questionsToBeInvalid);
                }
            }
            return 8;
        }

        public List<Identity> CalculateValidationChanges()
        {
            return new List<Identity>();
        }

        public List<Identity> CalculateConditionChanges()
        {
            throw new NotImplementedException();
        }
    }

    public static class Helpers
    {
        public static T[] Shrink<T>(this IEnumerable<T> vector)
        {
            var enumerable = vector as T[] ?? vector.ToArray();
            return enumerable.Take(enumerable.Count() - 1).ToArray();
        }
    }
}
// ReSharper restore InconsistentNaming

