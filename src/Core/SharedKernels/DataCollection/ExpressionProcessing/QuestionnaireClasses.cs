using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
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

        protected Dictionary<Identity, Func<bool>> validationExpressions = new Dictionary<Identity, Func<bool>>();
        protected Dictionary<Guid, bool?> enablementStatus = new Dictionary<Guid, bool?>();

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
        public long? persons_count;

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
        }

        public IValidatable CopyMembers()
        {
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
            validationExpressions.Add(new Identity(IdOf.age, this.rosterVector), age_IsValid);
            validationExpressions.Add(new Identity(IdOf.food, this.rosterVector), food_IsValid);
        }

        private QuestionnaireLevel parent;

        public string id { get { return this.parent.id; } }

        public long? persons_count { get { return this.parent.persons_count; } }

        public string name { get; set; }
        public string person_id { get; set; }
        public decimal? marital_status { get; set; }
        public decimal[][] married_with { get; set; }
        public long? age { get; set; }
        public DateTime? date { get; set; }
        public decimal? sex { get; set; }
        public decimal? role { get; set; }
        public decimal[] food { get; set; }
        public decimal? has_job { get; set; }
        public string job_title { get; set; }
        public decimal[] best_job_owner { get; set; }

        //generated
        private bool age_IsValid()
        {
            // person should not be too young and too old
            return age >= 0 && age < 100;
        }

        //generated
        private bool food_IsValid()
        {
            // children should not drink alcohol
            return food == null || (food.Contains(38) && role == 3 && age >= 21);
        }


        private void Validate(IEnumerable<HhMember> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var validationExpression in validationExpressions)
            {
                try
                {
                    if (validationExpression.Value())
                        questionsToBeValid.Add(validationExpression.Key);
                    else
                        questionsToBeInvalid.Add(validationExpression.Key);
                }
                catch (Exception)
                {
                    // failed to execute are treated as valid
                    questionsToBeValid.Add(validationExpression.Key);
                }
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
                best_job_owner = this.best_job_owner,
                person_id = this.person_id,
                marital_status = this.marital_status,
                married_with = this.married_with
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

        public long? persons_count
        {
            get { return this.parent.persons_count; }
        }

        public string name
        {
            get { return this.parent.name; }
        }

        public long? age
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

        public decimal[] best_job_owner
        {
            get { return this.parent.best_job_owner; }
        }

        public string person_id
        {
            get { return this.parent.person_id; }
        }

        public decimal? marital_status
        {
            get { return this.parent.marital_status; }
        }

        public decimal[][] married_with
        {
            get { return this.parent.married_with; }
        }

        public long times_per_week;
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
        public static readonly Guid person_id = Guid.Parse("9a5b5a4c-1746-4c31-a095-90a8d23e1a65");
        public static readonly Guid marital_status = Guid.Parse("0b84fcf2-e21c-4e9d-8257-7a9f988cb25b");
        public static readonly Guid married_with = Guid.Parse("b4434cee-3df0-47b2-9e27-ed81bd356d71");

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
}
