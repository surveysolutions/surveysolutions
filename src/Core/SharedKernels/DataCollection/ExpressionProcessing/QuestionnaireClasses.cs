using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    // ReSharper disable InconsistentNaming

    public class QuestionnaireLevel : AbstractConditionalLevel<QuestionnaireLevel>, IValidatable
    {
        public QuestionnaireLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : base(rosterVector, rosterKey, getInstances)
        {
            EnablementStates.Add(id_state.ItemId, id_state);
            EnablementStates.Add(edu_visit_state.ItemId, edu_visit_state);
            //EnablementStates.Add(persons_count_state.ItemId, persons_count_state);
        }

        private string @__id;
        private readonly ConditionalState id_state = new ConditionalState(IdOf.id);
        public string id
        {
            get { return id_state.State == State.Enabled ? @__id : String.Empty; }
            set { @__id = value; }
        }

        private long? @__persons_count;
        private readonly ConditionalState persons_count_state = new ConditionalState(IdOf.persons_count);
        public long? persons_count
        {
            get { return persons_count_state.State == State.Enabled ? @__persons_count : null; }
            set { @__persons_count = value; }
        }

        private decimal? @__edu_visit;
        private readonly ConditionalState edu_visit_state = new ConditionalState(IdOf.edu_visit);
        public decimal? edu_visit
        {
            get { return edu_visit_state.State == State.Enabled ? @__edu_visit : null; }
            set { @__edu_visit = value; }
        }

        public IValidatable CopyMembers()
        {
            var level = new QuestionnaireLevel(this.RosterVector, this.RosterKey, this.GetInstances);
            
            foreach (var conditionalState in level.EnablementStates)
            {
                var oldState = this.EnablementStates[conditionalState.Key];
                conditionalState.Value.State = oldState.State;
                conditionalState.Value.PreviousState = oldState.PreviousState;
            }

            ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions);
            InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions);

            level.id = this.@__id;
            level.persons_count = this.@__persons_count;

            return level;
        }

        private bool edu_visit_IsEnabled()
        {
            return true;
        }

        public void SetParent(IValidatable parentLevel)
        {
        }

        public IValidatable GetParent()
        {
            return null;
        }

        public void CalculateValidationChanges(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
        }

        protected override IEnumerable<Action> ConditionExpressions
        {
            get
            {
                return new[]
                {
                    Verifier(edu_visit_IsEnabled, edu_visit_state.ItemId, edu_visit_state)
                };
            }
        }

        public HhMember_type[] hhMembers
        {
            get
            {
                var rosters = this.GetInstances(this.RosterKey);
                return rosters == null ? new HhMember_type[0] : rosters.Select(x => x as HhMember_type).ToArray();
            }
        }

        public Education_type[] educations
        {
            get
            {
                var rosters = this.GetInstances(this.RosterKey);
                return rosters == null ? new Education_type[0] : rosters.Select(x => x as Education_type).ToArray();
            }
        }
    }
    
    //roster first level
    public class HhMember_type : AbstractRosterLevel<HhMember_type>, IValidatable
    {
        public HhMember_type(decimal[] rosterVector, Identity[] rosterKey, QuestionnaireLevel parent, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : this(rosterVector, rosterKey, getInstances)
        {
            this.@__parent = parent;
        }

        public HhMember_type(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : base(rosterVector, rosterKey, getInstances)
        {
            validationExpressions.Add(new Identity(IdOf.name, this.RosterVector), new Func<bool>[] { name_IsMandatory });
            validationExpressions.Add(new Identity(IdOf.age, this.RosterVector), new Func<bool>[] { age_IsValid });
            validationExpressions.Add(new Identity(IdOf.food, this.RosterVector), new Func<bool>[] { food_IsValid });
            validationExpressions.Add(new Identity(IdOf.role, this.RosterVector), new Func<bool>[] { role_IsValid, role2_IsValid });
            validationExpressions.Add(new Identity(IdOf.married_with, this.RosterVector), new Func<bool>[] { married_with_IsValid });

            EnablementStates.Add(age_state.ItemId, age_state);
            EnablementStates.Add(married_with_state.ItemId, married_with_state);
            EnablementStates.Add(has_job_state.ItemId, has_job_state);
            EnablementStates.Add(job_title_state.ItemId, job_title_state);
            EnablementStates.Add(best_job_owner_state.ItemId, best_job_owner_state);
            EnablementStates.Add(food_state.ItemId, food_state);
            EnablementStates.Add(person_id_state.ItemId, person_id_state);
            EnablementStates.Add(marital_status_state.ItemId, marital_status_state);
            EnablementStates.Add(group_state.ItemId, group_state);
        }

        private QuestionnaireLevel @__parent;

        public HhMember_type[] hhMembers
        {
            get
            {
                var rosters = this.GetInstances(this.RosterKey);
                return rosters == null ? new HhMember_type[0] : rosters.Select(x => x as HhMember_type).ToArray();
            }
        }

        public FoodConsumption_type[] foods
        {
            get
            {
                var rosters = this.GetInstances(this.RosterKey);
                return rosters == null ? new FoodConsumption_type[0] : rosters.Select(x => x as FoodConsumption_type).ToArray();
            }
        }

        public string id { get { return this.@__parent.id; } }

        public long? persons_count { get { return this.@__parent.persons_count; } }

        public decimal? edu_visit { get { return this.@__parent.edu_visit; } }

        
        public string name { get; set; }

        public DateTime? date { get; set; }

        public decimal? sex { get; set; }

        public decimal? role { get; set; }

        public string person_id
        {
            get { return person_id_state.State == State.Enabled ? this.@__personId : null; }
            set { this.@__personId = value; }
        }

        public decimal? marital_status
        {
            get { return marital_status_state.State == State.Enabled ? this.@__maritalStatus : null; }
            set { this.@__maritalStatus = value; }
        }

        public decimal[][] married_with
        {
            get { return married_with_state.State == State.Enabled ? this.@__marriedWith : null; }
            set { this.@__marriedWith = value; }
        }

        public long? age
        {
            get { return age_state.State == State.Enabled ? this.@__age : null; }
            set { this.@__age = value; }
        }

        public decimal[] food
        {
            get { return food_state.State == State.Enabled ? this.@__food : null; }
            set { this.@__food = value; }
        }

        public decimal? has_job
        {
            get { return has_job_state.State == State.Enabled ? this.@__hasJob : null; }
            set { this.@__hasJob = value; }
        }

        public string job_title
        {
            get { return job_title_state.State == State.Enabled ? this.@__jobTitle : null; }
            set { this.@__jobTitle = value; }
        }

        public decimal[] best_job_owner
        {
            get { return best_job_owner_state.State == State.Enabled ? this.@__bestJobOwner : null; }
            set { this.@__bestJobOwner = value; }
        }

        private ConditionalState age_state = new ConditionalState(IdOf.age);
        private ConditionalState married_with_state = new ConditionalState(IdOf.married_with);
        private ConditionalState has_job_state = new ConditionalState(IdOf.has_job);
        private ConditionalState job_title_state = new ConditionalState(IdOf.job_title);
        private ConditionalState best_job_owner_state = new ConditionalState(IdOf.best_job_owner);
        private ConditionalState food_state = new ConditionalState(IdOf.food);
        private ConditionalState group_state = new ConditionalState(IdOf.groupId, ItemType.Group);
        private ConditionalState person_id_state = new ConditionalState(IdOf.person_id);
        private ConditionalState marital_status_state = new ConditionalState(IdOf.marital_status);

        private long? @__age;
        private decimal[][] @__marriedWith;
        private decimal? @__hasJob;
        private string @__jobTitle;
        private decimal[] @__bestJobOwner;
        private decimal[] @__food;
        private string @__personId;
        private decimal? @__maritalStatus;

        protected override IEnumerable<Action> ConditionExpressions
        {
            get
            {
                return new[]
                {
                    Verifier(age_IsEnabledIf, age_state.ItemId, age_state),
                    Verifier(group_IsEnabledIf, group_state.ItemId, group_state),
                    Verifier(IsEnabledIfParentIs, person_id_state.ItemId, person_id_state),
                    Verifier(IsEnabledIfParentIs, marital_status_state.ItemId, marital_status_state),
                    Verifier(married_with_IsEnabledIf, married_with_state.ItemId, married_with_state),
                    Verifier(food_IsEnabledIf, food_state.ItemId, food_state),
                    Verifier(has_job_IsEnabledIf, has_job_state.ItemId, has_job_state),
                    Verifier(job_title_IsEnabledIf, job_title_state.ItemId, job_title_state),
                    Verifier(best_job_owner_IsEnabledIf, best_job_owner_state.ItemId, best_job_owner_state)
                };
            }
        }

        private bool age_IsEnabledIf()
        {
            return name.ToLower().StartsWith("a");
        }
        
        private bool group_IsEnabledIf()
        {
            return (age > 16);
        }

        private bool married_with_IsEnabledIf()
        {
            return marital_status == 2 && persons_count > 1;
        }

        private bool food_IsEnabledIf()
        {
            return role == 2 && sex == 2;
        }

        private bool has_job_IsEnabledIf()
        {
            return age > 16;
        }

        private bool job_title_IsEnabledIf()
        {
            return has_job == 1;
        }

        private bool best_job_owner_IsEnabledIf()
        {
            return has_job == 2;
        }

        private bool age_IsValid()
        {
            return age >= 0 && age < 100;
        }

        private bool married_with_IsValid()
        {
            return !married_with.Any(x => x.SequenceEqual(me));
        }

        private bool name_IsMandatory()
        {
            return !IsEmptyAnswer(name);
        }

        private bool food_IsValid()
        {
            return food == null || !(food.Contains(38) && role == 3 && age >= 21);
        }

        private bool role_IsValid()
        {
            // children should not drink alcohol
            return (role == 1 && hhMembers.Count(x => x.role == 1) == 1) || role != 1;
        }

        private bool role2_IsValid()
        {
            // children should not drink alcohol
            return (role == 3 && hhMembers.Where(x => x.role < 3).Any(x => x.age < age + 10)) || role != 3;
        }
        
        public IValidatable CopyMembers()
        {
            var level = new HhMember_type(this.RosterVector, this.RosterKey, this.GetInstances)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),

                name = this.name,
                date = this.date,
                sex = this.sex,
                role = this.role,
                // should be taken from fileds, not properties
                age = this.@__age,
                food = this.@__food,
                has_job = this.@__hasJob,
                job_title = this.@__jobTitle,
                best_job_owner = this.@__bestJobOwner,
                person_id = this.@__personId,
                marital_status = this.@__maritalStatus,
                married_with = this.@__marriedWith
            };
            foreach (var state in level.EnablementStates)
            {
                var originalState = this.EnablementStates[state.Key];
                state.Value.PreviousState = originalState.PreviousState;
                state.Value.State = originalState.State;
            }

            return level;
        }
        
        public void SetParent(IValidatable parentLevel)
        {
            this.@__parent = parentLevel as QuestionnaireLevel;
        }

        public IValidatable GetParent()
        {
            return this.@__parent;
        }

        public void CalculateValidationChanges(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(questionsToBeValid, questionsToBeInvalid);
        }
    }

    //roster second level
    public class FoodConsumption_type : AbstractRosterLevel<FoodConsumption_type>, IValidatable
    {
        public FoodConsumption_type(decimal[] rosterVector, Identity[] rosterKey, HhMember_type parent, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : this(rosterVector, rosterKey, getInstances)
        {
            this.@__parent = parent;
        }

        public FoodConsumption_type(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : base(rosterVector, rosterKey , getInstances)
        {
            validationExpressions.Add(new Identity(IdOf.times_per_week, this.RosterVector), new Func<bool>[]{times_per_week_validation});

            EnablementStates.Add(price_for_food_state.ItemId, price_for_food_state);
        }

        private HhMember_type @__parent;

        public HhMember_type[] hhMembers
        {
            get { return @__parent.hhMembers; }
        }

        public FoodConsumption_type[] foodConsumption
        {
            get
            {
                var rosters = this.GetInstances(this.RosterKey);
                return rosters == null ? new FoodConsumption_type[0] : rosters.Select(x => x as FoodConsumption_type).ToArray();
            }
        }

        public string id { get { return this.@__parent.id; } }

        public long? persons_count { get { return this.@__parent.persons_count; } }

        public decimal? edu_visit { get { return this.@__parent.edu_visit; } }

        public string name
        {
            get { return this.@__parent.name; }
        }

        public long? age
        {
            get { return this.@__parent.age; }
        }

        public DateTime? date
        {
            get { return this.@__parent.date; }
        }

        public decimal? sex
        {
            get { return this.@__parent.sex; }
        }

        public decimal? role
        {
            get { return this.@__parent.role; }
        }

        public decimal[] food
        {
            get { return this.@__parent.food; }
        }

        public decimal? has_job
        {
            get { return this.@__parent.has_job; }
        }

        public string job_title
        {
            get { return this.@__parent.job_title; }
        }

        public decimal[] best_job_owner
        {
            get { return this.@__parent.best_job_owner; }
        }

        public string person_id
        {
            get { return this.@__parent.person_id; }
        }

        public decimal? marital_status
        {
            get { return this.@__parent.marital_status; }
        }

        public decimal[][] married_with
        {
            get { return this.@__parent.married_with; }
        }

        public long times_per_week { get; set; }

        private bool times_per_week_validation()
        {
            return times_per_week > 0 && times_per_week < 7 * 5;
        }

        public decimal? price_for_food
        {
            get { return price_for_food_state.State == State.Enabled ? this.priceForFood : null; }
            set { this.priceForFood = value; }
        }

        private ConditionalState price_for_food_state = new ConditionalState(IdOf.price_for_food);
        private decimal? priceForFood;

        private bool price_for_food_IsEnabledIf()
        {
            return times_per_week > 0;
        }
        
        public void CalculateValidationChanges(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(questionsToBeValid, questionsToBeInvalid);
        }

        public IValidatable CopyMembers()
        {
            var level = new FoodConsumption_type(this.RosterVector, this.RosterKey, this.GetInstances)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),


                price_for_food = this.priceForFood,
                times_per_week = this.times_per_week
            };

            foreach (var state in level.EnablementStates)
            {
                var originalState = this.EnablementStates[state.Key];
                state.Value.PreviousState = originalState.PreviousState;
                state.Value.State = originalState.State;
            }

            return level;
        }

        public void SetParent(IValidatable parentLevel)
        {
            this.@__parent = parentLevel as HhMember_type;
        }

        public IValidatable GetParent()
        {
            return this.@__parent;
        }

        protected override IEnumerable<Action> ConditionExpressions
        {
            get
            {
                return new[]
                {
                    Verifier(price_for_food_IsEnabledIf,price_for_food_state.ItemId, price_for_food_state)
                };
            }
        }
    }

    public class Education_type : AbstractRosterLevel<Education_type>, IValidatable
    {
        public Education_type(decimal[] rosterVector, Identity[] rosterKey, QuestionnaireLevel parent, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : this(rosterVector, rosterKey, getInstances)
        {
            this.@__parent = parent;
        }

        public Education_type(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], IEnumerable<IValidatable>> getInstances)
            : base(rosterVector, rosterKey, getInstances)
        {
        }

        private QuestionnaireLevel @__parent;

        public Education_type[] educations
        {
            get
            {
                var rosters = this.GetInstances(this.RosterKey);
                return rosters == null ? new Education_type[0] : rosters.Select(x => x as Education_type).ToArray();
            }
        }

        public string id { get { return this.@__parent.id; } }

        public long? persons_count { get { return this.@__parent.persons_count; } }

        public decimal? edu_visit { get { return this.@__parent.edu_visit; } }

        public decimal? edu { get; set; }


        protected override IEnumerable<Action> ConditionExpressions
        {
            get
            {
                return new Action[]
                {
                };
            }
        }

        public IValidatable CopyMembers()
        {
            var level = new Education_type(this.RosterVector, this.RosterKey, this.GetInstances)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),

                edu = this.edu
            };
            foreach (var state in level.EnablementStates)
            {
                var originalState = this.EnablementStates[state.Key];
                state.Value.PreviousState = originalState.PreviousState;
                state.Value.State = originalState.State;
            }

            return level;
        }

        public void SetParent(IValidatable parentLevel)
        {
            this.@__parent = parentLevel as QuestionnaireLevel;
        }

        public IValidatable GetParent()
        {
            return this.@__parent;
        }

        public void CalculateValidationChanges(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            this.Validate(questionsToBeValid, questionsToBeInvalid);
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
        public static readonly Guid edu = Guid.Parse("4031fc8f-cc03-4803-882a-7ce882f68df5");
        public static readonly Guid edu_visit = Guid.Parse("52f2a00a-9d15-454e-8702-f9f15871f463");
        
        public static readonly Guid questionnaire = Guid.Parse("72897e3f-3dc8-4115-81e0-8a9c1cadec2d");
        public static readonly Guid hhMember = Guid.Parse("69b02bcf-0ed1-4b5a-80bb-bd465ab096da");
        public static readonly Guid foodConsumption = Guid.Parse("6df41d95-c785-4452-8303-bed3985d4c20");
        public static readonly Guid jobActivity = Guid.Parse("7556ddf0-457a-4a9b-a021-45ac3bad05a8");

        public static readonly Guid groupId = Guid.Parse("039ed69e-5583-46af-b983-488568f20e1c");
        public static readonly Guid fixedId = Guid.Parse("a7b0d842-0355-4eab-a943-968c9c013d97");

        public static readonly Guid[] eduScopeIds = new[] { fixedId };
        public static readonly Guid[] hhMemberScopeIds = new[] { persons_count };
        public static readonly Guid[] foodConsumptionIds = new[] { persons_count, food };

        //somehow should be generated
        public static Dictionary<Guid, Guid[]> conditionalDependencies = new Dictionary<Guid, Guid[]>()
        {
            { id, new Guid[] { } },
            { persons_count, new Guid[] { married_with } },
            { edu_visit, new Guid[] { fixedId } },

            { name, new Guid[] { age } },
            { age, new Guid[] { groupId, has_job } },
            { groupId, new Guid[] { person_id, marital_status, married_with } },
            
            { person_id, new Guid[] { } },
            { marital_status, new Guid[] { married_with } },
            { married_with, new Guid[] { } },
            { has_job, new Guid[] { job_title, best_job_owner } },
            { job_title, new Guid[] { } },
            { best_job_owner, new Guid[] { } },

            { sex, new Guid[] { food } },
            { role, new Guid[] { food, edu_visit } },
            { food, new Guid[] { } },

            { times_per_week, new Guid[] { price_for_food } },
            { price_for_food, new Guid[] { } },

            { fixedId, new Guid[] { edu } },
        };

        public static Dictionary<Guid, Guid[]> parentsMap = new Dictionary<Guid, Guid[]>
        {
            { id, new []{questionnaire} },
            { persons_count, new []{questionnaire}},
            { edu_visit, new []{questionnaire} },
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
            { person_id, hhMemberScopeIds },
            { marital_status, hhMemberScopeIds },
            { married_with, hhMemberScopeIds },
            { edu, eduScopeIds },

            //groups
            { groupId, hhMemberScopeIds },
            { fixedId, eduScopeIds },
            { hhMember, hhMemberScopeIds },
            { foodConsumption, foodConsumptionIds },
            
        };

        public static Dictionary<Guid, Guid[]> rostersIdToScopeMap = new Dictionary<Guid, Guid[]>
        {
            { fixedId, eduScopeIds },
            { hhMember, hhMemberScopeIds },
            { foodConsumption, foodConsumptionIds },
            { jobActivity, hhMemberScopeIds }
        };
    }
}
