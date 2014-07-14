using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public interface IValidatable
    {
        IValidatable CopyMembers();

        Identity[] GetRosterKey();
        void SetParent(IValidatable parentLevel);
        IValidatable GetParent();
    }

    public interface IValidatableRoster
    {
        void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid);
        void RunConditions(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);
    }

    public enum State
    {
        Unknown = 0,
        Enabled = 1,
        Disabled = 2
    }
    public enum ItemType
    {
        Question = 1,
        Group = 10
    }

    public class ConditionalState
    {
        public ConditionalState(Guid itemId, ItemType type = ItemType.Question)
        {
            this.Type = type;
            this.ItemId = itemId;
            this.State = State.Enabled;
            PreviousState = State.Enabled;
        }

        public Guid ItemId { get; set; }
        public ItemType Type { get; set; }
        public State State { get; set; }
        public State PreviousState { get; set; }
    }
    ;
    public abstract class AbstractConditionalLevel<T>  where T : IValidatable
    {
        public decimal[] RosterVector { get; private set; }
        protected List<ConditionalState> enablementStatus = new List<ConditionalState>();

        protected abstract IEnumerable<Action<T[]>> ConditionExpressions { get; }

        protected AbstractConditionalLevel(decimal[] rosterVector)
        {
            this.RosterVector = rosterVector;
        }

        private State RunConditionExpression(Func<T[], bool> expression, T[] rosters)
        {
            try
            {
                return expression(rosters) ? State.Enabled : State.Disabled;
            }
            catch
            {
                return State.Disabled;
            }
        }

        protected void DisableAllDependentQuestions(Guid itemId)
        {
            if (!IdOf.conditionalDependencies.ContainsKey(itemId) || !IdOf.conditionalDependencies[itemId].Any()) return;

            var stack = new Queue<Guid>(IdOf.conditionalDependencies[itemId]);
            while (stack.Any())
            {
                var id = stack.Dequeue();
                var questionState = this.enablementStatus.FirstOrDefault(x => x.ItemId == id);
                if (questionState != null)
                {
                    questionState.State = State.Disabled;
                }

                if (IdOf.conditionalDependencies.ContainsKey(id) && IdOf.conditionalDependencies[id].Any())
                {
                    foreach (var dependentQuestionId in IdOf.conditionalDependencies[id])
                    {
                        stack.Enqueue(dependentQuestionId);
                    }
                }
            }
        }

        protected Action<T[]> Verifier(Func<T[], bool> isEnabled, Guid questionId, ConditionalState questionState)
        {
            return rosters =>
            {
                if (questionState.State == State.Disabled) return;
                questionState.State = this.RunConditionExpression(isEnabled, rosters);
                if (questionState.State == State.Disabled)
                {
                    this.DisableAllDependentQuestions(questionId);
                }
            };
        }

        public void EvaluateConditions(T[] roters, List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled
            , List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            foreach (var state in this.enablementStatus)
            {
                state.PreviousState = state.State;
                state.State = State.Unknown;
            }

            foreach (Action<T[]> verifier in this.ConditionExpressions)
            {
                verifier(roters);
            }

            var questionsToBeEnabledArray = this.enablementStatus
                .Where(x => x.State == State.Enabled && x.State != x.PreviousState && x.Type == ItemType.Question)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToArray();

            var questionsToBeDisabledArray = this.enablementStatus
                .Where(x => x.State == State.Disabled && x.State != x.PreviousState && x.Type == ItemType.Question)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToArray();

            var groupsToBeEnabledArray = this.enablementStatus
                .Where(x => x.State == State.Enabled && x.State != x.PreviousState && x.Type == ItemType.Group)
                .Select(x => new Identity(x.ItemId, this.RosterVector))
                .ToArray();

            var groupsToBeDisabledArray = this.enablementStatus
                .Where(x => x.State == State.Disabled && x.State != x.PreviousState && x.Type == ItemType.Group)
                .Select(x => new Identity(x.ItemId, this.RosterVector));

            questionsToBeEnabled.AddRange(questionsToBeEnabledArray);
            questionsToBeDisabled.AddRange(questionsToBeDisabledArray);
            groupsToBeEnabled.AddRange(groupsToBeEnabledArray);
            groupsToBeDisabled.AddRange(groupsToBeDisabledArray);
        }
    }

    public abstract class AbstractRosterLevel<T> : AbstractConditionalLevel<T>, IValidatableRoster where T : IValidatable
    {
        public Identity[] RosterKey { get; private set; }
    

        protected Dictionary<Identity, Func<T[], bool>> validationExpressions = new Dictionary<Identity, Func<T[], bool>>();

        protected AbstractRosterLevel(decimal[] rosterVector, Identity[] rosterKey)
            : base(rosterVector)
        {
            this.RosterKey = rosterKey;
        }

        public abstract void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid);

        public abstract void RunConditions(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeEnabled,
            List<Identity> questionsToBeDisabled
            , List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled);
    }

    public class QuestionnaireLevel : AbstractConditionalLevel<QuestionnaireLevel>, IValidatable
    {
        protected Dictionary<Guid, bool?> enablementStatus = new Dictionary<Guid, bool?>();

        public Identity[] RosterKey { get; private set; }

        public QuestionnaireLevel(decimal[] rosterVector, Identity[] rosterKey)
            : base(rosterVector)
        {
            this.RosterKey = rosterKey;
        }

        public string id { get; set; }
        public long? persons_count { get; set; }

        public IValidatable CopyMembers()
        {
            var level = new QuestionnaireLevel(this.RosterVector, this.RosterKey)
            {
                id = this.id,
                persons_count = this.persons_count
            };

            return level;
        }

        public Identity[] GetRosterKey()
        {
            return this.RosterKey;
        }

        public void SetParent(IValidatable parentLevel)
        {
        }

        public IValidatable GetParent()
        {
            return null;
        }

        protected override IEnumerable<Action<QuestionnaireLevel[]>> ConditionExpressions
        {
            get
            {
                return Enumerable.Empty<Action<QuestionnaireLevel[]>>();
            }
        }

        public void Validate(List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
        }

        public void RunConditions(List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled, List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            this.EvaluateConditions(new QuestionnaireLevel[0], questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled, groupsToBeDisabled);
        }
    }

    public class HhMember : AbstractRosterLevel<HhMember>, IValidatable
    {
        public HhMember(decimal[] rosterVector, Identity[] rosterKey, QuestionnaireLevel parent)
            : this(rosterVector, rosterKey)
        {
            this.parent = parent;
        }

        public HhMember(decimal[] rosterVector, Identity[] rosterKey)
            : base(rosterVector, rosterKey)
        {
            validationExpressions.Add(new Identity(IdOf.age, this.RosterVector), age_IsValid);
            validationExpressions.Add(new Identity(IdOf.food, this.RosterVector), food_IsValid);
            validationExpressions.Add(new Identity(IdOf.role, this.RosterVector), role_IsValid);
            
            enablementStatus.AddRange(new[]
            {
                age_state, married_with_state, has_job_state, job_title_state, best_job_owner_state,
                food_state, person_id_state, marital_status_state, group_state
            });
        }

        private QuestionnaireLevel parent;

        public string id { get { return this.parent.id; } }

        public long? persons_count { get { return this.parent.persons_count; } }

        public string name { get; set; }

        public DateTime? date { get; set; }

        public decimal? sex { get; set; }

        public decimal? role { get; set; }

        public string person_id
        {
            get { return person_id_state.State == State.Enabled ? this.personId : null; }
            set { this.personId = value; }
        }

        public decimal? marital_status
        {
            get { return marital_status_state.State == State.Enabled ? this.maritalStatus : null; }
            set { this.maritalStatus = value; }
        }

        public decimal[][] married_with
        {
            get { return married_with_state.State == State.Enabled? this.marriedWith : null; }
            set { this.marriedWith = value; }
        }

        public long? age
        {
            get { return age_state.State == State.Enabled ? this.age1 : null; }
            set { this.age1 = value; }
        }

        public decimal[] food
        {
            get { return food_state.State == State.Enabled ? this.food1 : null; }
            set { this.food1 = value; }
        }

        public decimal? has_job
        {
            get { return has_job_state.State == State.Enabled ? this.hasJob : null; }
            set { this.hasJob = value; }
        }

        public string job_title
        {
            get { return job_title_state.State == State.Enabled ? this.jobTitle : null; }
            set { this.jobTitle = value; }
        }

        public decimal[] best_job_owner
        {
            get { return best_job_owner_state.State == State.Enabled ? this.bestJobOwner : null; }
            set { this.bestJobOwner = value; }
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

        private long? age1;
        private decimal[][] marriedWith;
        private decimal? hasJob;
        private string jobTitle;
        private decimal[] bestJobOwner;
        private decimal[] food1;
        private string personId;
        private decimal? maritalStatus;

        protected override IEnumerable<Action<HhMember[]>> ConditionExpressions
        {
            get
            {
                return new[]
                {
                    Verifier(age_IsEnabledIf, age_state.ItemId, age_state),
                    Verifier(group_IsEnabledIf, group_state.ItemId, group_state),
                    Verifier(married_with_IsEnabledIf, married_with_state.ItemId, married_with_state),
                    Verifier(food_IsEnabledIf, food_state.ItemId, food_state),
                    Verifier(has_job_IsEnabledIf, has_job_state.ItemId, has_job_state),
                    Verifier(job_title_IsEnabledIf, job_title_state.ItemId, job_title_state),
                    Verifier(best_job_owner_IsEnabledIf, best_job_owner_state.ItemId, best_job_owner_state)
                };
            }
        }

        //generated
        private bool age_IsEnabledIf(HhMember[] roster)
        {
            return name.ToLower().StartsWith("a");
        }

        //generated
        private bool married_with_IsEnabledIf(HhMember[] roster)
        {
            return marital_status == 2 && persons_count > 2;
        }

        //generated
        private bool group_IsEnabledIf(HhMember[] roster)
        {
            return (age > 16);
        }

        //generated
        private bool food_IsEnabledIf(HhMember[] roster)
        {
            return role == 2 && sex == 2;
        }

        //generated
        private bool has_job_IsEnabledIf(HhMember[] roster)
        {
            return age > 16;
        }

        //generated
        private bool job_title_IsEnabledIf(HhMember[] roster)
        {
            return has_job == 1;
        }

        //generated
        private bool best_job_owner_IsEnabledIf(HhMember[] roster)
        {
            return has_job == 2;
        }

        //generated
        private bool age_IsValid(HhMember[] roster)
        {
            // person should not be too young and too old
            return age >= 0 && age < 100;
        }

        //generated
        private bool food_IsValid(HhMember[] roster)
        {
            // children should not drink alcohol
            return food == null || (food.Contains(38) && role == 3 && age >= 21);
        }

        private bool role_IsValid(HhMember[] roster)
        {
            // children should not drink alcohol
            return role == 1 && roster.Count(x => x.role == 1) == 1;
        }


        private void Validate(HhMember[] roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            foreach (var validationExpression in validationExpressions)
            {
                try
                {
                    if (validationExpression.Value(roters))
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
            this.Validate(rosters.Select(x => x as HhMember).ToArray(), questionsToBeValid, questionsToBeInvalid);
        }

        public override void RunConditions(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled
            , List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            this.EvaluateConditions(rosters.Select(x => x as HhMember).ToArray(), questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled,groupsToBeDisabled );
        }

        public IValidatable CopyMembers()
        {
            var level = new HhMember(this.RosterVector, this.RosterKey)
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
            return this.RosterKey;
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

    public class FoodConsumption : AbstractRosterLevel<FoodConsumption>, IValidatable
    {
        public FoodConsumption(decimal[] rosterVector, Identity[] rosterKey, HhMember parent)
            : this(rosterVector, rosterKey)
        {
            this.parent = parent;
        }

        public FoodConsumption(decimal[] rosterVector, Identity[] rosterKey)
            : base(rosterVector, rosterKey)
        {
            validationExpressions.Add(new Identity(IdOf.times_per_week, this.RosterVector), times_per_week_validation);
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

        public long times_per_week { get; set; }

        private bool times_per_week_validation(FoodConsumption[] rosters)
        {
            return times_per_week > 0 && times_per_week < 7*5;
        }

        public decimal? price_for_food
        {
            get { return price_for_food_state.State == State.Enabled ? this.priceForFood : null; }
            set { this.priceForFood = value; }
        }

        private ConditionalState price_for_food_state = new ConditionalState(IdOf.age);
        private decimal? priceForFood;

        private bool price_for_food_IsEnabledIf(FoodConsumption[] foodConsumptions)
        {
            return times_per_week > 0;
        }

        private void Validate(IEnumerable<FoodConsumption> roters, List<Identity> questionsToBeValid, List<Identity> questionsToBeInvalid)
        {
            
        }

        public override void Validate(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid)
        {
            this.Validate(rosters.Cast<FoodConsumption>(), questionsToBeValid, questionsToBeInvalid);
        }

        public override void RunConditions(IEnumerable<IValidatable> rosters, List<Identity> questionsToBeEnabled, List<Identity> questionsToBeDisabled
            , List<Identity> groupsToBeEnabled, List<Identity> groupsToBeDisabled)
        {
            this.EvaluateConditions(rosters.Select(x => x as FoodConsumption).ToArray(), questionsToBeEnabled, questionsToBeDisabled, groupsToBeEnabled, groupsToBeDisabled);
        }

        public IValidatable CopyMembers()
        {
            var level = new FoodConsumption(this.RosterVector, this.RosterKey)
            {
                price_for_food = this.price_for_food,
                times_per_week = this.times_per_week
            };

            return level;
        }

        public Identity[] GetRosterKey()
        {
            return this.RosterKey;
        }

        public void SetParent(IValidatable parentLevel)
        {
            this.parent = parentLevel as HhMember;
        }

        public IValidatable GetParent()
        {
            return this.parent;
        }

        protected override IEnumerable<Action<FoodConsumption[]>> ConditionExpressions
        {
            get
            {
                return new[]
                {
                    Verifier(price_for_food_IsEnabledIf,price_for_food_state.ItemId,price_for_food_state)
                };
            }
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

        public static readonly Guid groupId = Guid.Parse("039ed69e-5583-46af-b983-488568f20e1c");

        public static Dictionary<Guid, Guid[]> conditionalDependencies = new Dictionary<Guid, Guid[]>()
        {
            { id, new Guid[] { } },
            { persons_count, new Guid[] { married_with } },
            { name, new Guid[] { age } },
            { age, new Guid[] { groupId, has_job } },
            { groupId, new Guid[] { person_id, marital_status, marital_status } },
            { person_id, new Guid[] { } },
            { marital_status, new Guid[] { married_with } },
            { married_with, new Guid[] { } },
            { has_job, new Guid[] { job_title, best_job_owner } },
            { job_title, new Guid[] { } },
            { best_job_owner, new Guid[] { } },

            { sex, new Guid[] { food } },
            { role, new Guid[] { food } },
            { food, new Guid[] { } },

            { times_per_week, new Guid[] { price_for_food } },
            { price_for_food, new Guid[] { } },
        };

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
            { person_id, hhMemberScopeIds },
            { marital_status, hhMemberScopeIds },
            { married_with, hhMemberScopeIds },
        };

        public static Dictionary<Guid, Guid[]> rostersIdToScopeMap = new Dictionary<Guid, Guid[]>
        {
            { hhMember, hhMemberScopeIds },
            { foodConsumption, foodConsumptionIds },
            { jobActivity, hhMemberScopeIds }
        };
    }
}
