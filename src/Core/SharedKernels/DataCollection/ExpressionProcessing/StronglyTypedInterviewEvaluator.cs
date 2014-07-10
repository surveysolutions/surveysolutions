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
            var emptyRosterVector = new decimal[0];
            var questionnaireIdentityKey = GetRosterKey(new[] { IdOf.questionnaire }, emptyRosterVector);

            var questionnaireLevel = new QuestionnaireLevel(emptyRosterVector, questionnaireIdentityKey);

            this.interviewScopes.Add(GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
        }

        public StronglyTypedInterviewEvaluator(Dictionary<string, IValidatable> interviewScopes)
        {
            this.interviewScopes = interviewScopes;
        }

        public static string GetRosterStringKey(Identity[] scopeIds)
        {
            return string.Join((string) "$", (IEnumerable<string>) scopeIds.Select(ConversionHelper.ConvertIdentityToString));
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

            decimal[] rosterVector = GetRosterVector(outerRosterVector, rosterInstanceId);

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

        public static decimal[] GetRosterVector(decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            var outerRosterList = outerRosterVector.ToList();
            outerRosterList.Add(rosterInstanceId);
            return outerRosterList.ToArray();
        }

        public void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!IdOf.rostersIdToScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = GetRosterKey(IdOf.rostersIdToScopeMap[rosterId], rosterVector);
            var dependentRosters = this.interviewScopes.Keys.Where(x => x.StartsWith(GetRosterStringKey((rosterIdentityKey)))).ToArray();
            foreach (var rosterKey in dependentRosters)
            {
                this.interviewScopes.Remove(rosterKey);
            }
        }

        public void UpdateIntAnswer(Guid questionId, decimal[] rosterVector, int answer)
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

            if (questionId == IdOf.best_job_owner)
            {
                (targetLevel as HhMember).best_job_owner = answer;
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
        }

        public void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] selectedPropagationVectors)
        {
            var targetLevel = this.GetRosterToUpdateAnswer(questionId, rosterVector);
            if (targetLevel == null) return;
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

    public interface IValidatable
    {
        void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);

        IValidatable CopyMembers();

        Identity[] GetRosterKey();
        void SetParent(IValidatable parentLevel);
        IValidatable GetParent();
    }

    public interface IValidatableRoster
    {
        void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
    }

    public abstract class AbstractRosterLevel : IValidatableRoster
    {
        public decimal[] rosterVector;

        public Identity[] rosterKey;

        protected AbstractRosterLevel(decimal[] rosterVector, Identity[] rosterKey)
        {
            this.rosterVector = rosterVector;
            this.rosterKey = rosterKey;
        }

        public abstract void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid);
    }

    public class QuestionnaireLevel : IValidatable
    {
        public decimal[] rosterVector;

        public Identity[] rosterKey;

        public QuestionnaireLevel(decimal[] rosterVector, Identity[] rosterKey)
        {
            this.rosterVector = rosterVector;
            this.rosterKey = rosterKey;
        }

        public string id;
        public int? persons_count;

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            //Members.Count(m => m.age > 15);
        }

        public IValidatable CopyMembers()
        {
            //var level = this.MemberwiseClone() as QuestionnaireLevel;

            var level = new QuestionnaireLevel(this.rosterVector, this.rosterKey)
            {
                id = this.id,
                persons_count = this.persons_count
            };

            return level;
        }

        public Identity[] GetRosterKey()
        {
            return this.rosterKey;
        }

        public void SetParent(IValidatable parentLevel)
        {
        }

        public IValidatable GetParent()
        {
            return null;
        }
    }

    public class HhMember : AbstractRosterLevel, IValidatable
    {
        public HhMember(decimal[] rosterVector, Identity[] rosterKey, QuestionnaireLevel parent)
            : this(rosterVector, rosterKey)
        {
            this.parent = parent;
        }

        public HhMember(decimal[] rosterVector, Identity[] rosterKey)
            : base(rosterVector, rosterKey)
        {
        }

        public QuestionnaireLevel parent;

        public string id
        {
            get { return this.parent.id; }
        }

        public int? persons_count
        {
            get { return this.parent.persons_count; }
        }

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
            return this.age >= 0 && this.age < 100;
        }

        private bool food_IsValid()
        {
            // children should not dring alcohol
            return this.food.Contains(38) && this.role == 3 && this.age >= 21;
        }

        private void Validate(IEnumerable<HhMember> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            //if (age_Validation().CanBeProcessed)
            {
                var ageIdentity = new Identity(IdOf.age, this.rosterVector);
                (this.age_IsValid() ? questionsToBeValid : questionsToBeInvalid).Add(ageIdentity);
            }
            //if (food_IsValid().CanBeProcessed)
            {
                var ageIdentity = new Identity(IdOf.age, this.rosterVector);
                (this.age_IsValid() ? questionsToBeValid : questionsToBeInvalid).Add(ageIdentity);
            }
        }


        public override void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(Enumerable.Empty<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }

        public IValidatable CopyMembers()
        {
            var level = new HhMember(this.rosterVector, this.rosterKey)
            {
                name = this.name,
                age = this.age,
                date = this.date,
                sex = this.sex,
                role = this.role,
                food = this.food,
                has_job = this.has_job,
                job_title = this.job_title,
                best_job_owner = this.best_job_owner
            };

            return level;
        }

        public Identity[] GetRosterKey()
        {
            return this.rosterKey;
        }

        public void SetParent(IValidatable parentLevel)
        {
            this.parent = parentLevel as QuestionnaireLevel;
        }

        public IValidatable GetParent()
        {
            return this.parent;
        }
    }

    public class FoodConsumption : AbstractRosterLevel, IValidatable
    {
        public FoodConsumption(decimal[] rosterVector, Identity[] rosterKey, HhMember parent)
            : this(rosterVector, rosterKey)
        {
            this.parent = parent;
        }

        public FoodConsumption(decimal[] rosterVector, Identity[] rosterKey)
            : base(rosterVector, rosterKey)
        {

        }

        private HhMember parent;

        public string id { get { return this.parent.id; } }

        public int? persons_count
        {
            get { return this.parent.persons_count; }
        }

        public string name
        {
            get { return this.parent.name; }
        }

        public int? age
        {
            get { return this.parent.age; }
        }

        public DateTime? date
        {
            get { return this.parent.date; }
        }

        public decimal? sex
        {
            get { return this.parent.sex; }
        }

        public decimal? role
        {
            get { return this.parent.role; }
        }

        public decimal[] food
        {
            get { return this.parent.food; }
        }

        public decimal? has_job
        {
            get { return this.parent.has_job; }
        }

        public string job_title
        {
            get { return this.parent.job_title; }
        }

        public decimal? best_job_owner
        {
            get { return this.parent.best_job_owner; }
        }


        public int times_per_week;
        public decimal price_for_food;


        private void Validate(IEnumerable<FoodConsumption> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid) { }

        public override void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<FoodConsumption>(), questionsToBeValid, questionsToBeInvalid);
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(Enumerable.Empty<HhMember>(), questionsToBeValid, questionsToBeInvalid);
        }


        public IValidatable CopyMembers()
        {
            //this.MemberwiseClone() as FoodConsumption;

            var level = new FoodConsumption(this.rosterVector, this.rosterKey)
            {
                price_for_food = this.price_for_food,
                times_per_week = this.times_per_week

            };

            return level;
        }

        public Identity[] GetRosterKey()
        {
            return this.rosterKey;
        }

        public void SetParent(IValidatable parentLevel)
        {
            this.parent = parentLevel as HhMember;
        }

        public IValidatable GetParent()
        {
            return this.parent;
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
            { id, new []{questionnaire} },
            { persons_count, new []{questionnaire}},
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
            { hhMember, hhMemberScopeIds },
            { foodConsumption, foodConsumptionIds },
            { jobActivity, hhMemberScopeIds }
        };
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

