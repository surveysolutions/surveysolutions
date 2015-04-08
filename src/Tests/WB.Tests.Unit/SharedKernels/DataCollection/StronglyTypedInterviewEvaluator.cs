using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;

// ReSharper disable InconsistentNaming

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    public class StronglyTypedInterviewEvaluator : AbstractInterviewExpressionState 
    {
        public StronglyTypedInterviewEvaluator()
        {
            var questionnaireLevelScope = new[] { IdOf.questionnaire };
            var questionnaireIdentityKey = Util.GetRosterKey(questionnaireLevelScope, Util.EmptyRosterVector);
            var questionnaireLevel = new QuestionnaireLevel(Util.EmptyRosterVector, questionnaireIdentityKey, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
            this.InterviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
        }

        private StronglyTypedInterviewEvaluator(Dictionary<string, IExpressionExecutable> interviewScopes, Dictionary<string, List<string>> siblingRosters)
        {
            var newScopes = interviewScopes.ToDictionary(interviewScope => interviewScope.Key, interviewScope => interviewScope.Value.CopyMembers(this.GetRosterInstances));

            var newSiblingRosters = siblingRosters
                .ToDictionary(
                    interviewScope => interviewScope.Key,
                    interviewScope => new List<string>(interviewScope.Value));

            //set parents
            foreach (var interviewScope in interviewScopes)
            {
                var parent = interviewScope.Value.GetParent();
                if (parent != null)
                    newScopes[interviewScope.Key].SetParent(newScopes[Util.GetRosterStringKey(parent.GetRosterKey())]);
            }

            this.InterviewScopes = newScopes;
            this.SiblingRosters = newSiblingRosters;
        }

        public override void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            if (!IdOf.parentScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            Guid[] rosterScopeIds = IdOf.parentScopeMap[rosterId];
            var rosterIdentityKey = Util.GetRosterKey(rosterScopeIds, rosterVector);
            string rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            if (this.InterviewScopes.ContainsKey(rosterStringKey))
            {
                return;
            }

            var rosterParentIdentityKey = outerRosterVector.Length == 0
                ? Util.GetRosterKey(new[] { IdOf.questionnaire }, new decimal[0])
                : Util.GetRosterKey(rosterScopeIds.Shrink(), outerRosterVector);

            var parent = this.InterviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            if (rosterId == IdOf.hhMember || rosterId == IdOf.jobActivity)
            {
                var parentHolder = parent as QuestionnaireLevel;
                var rosterLevel = new HhMember_type(rosterVector, rosterIdentityKey, parentHolder, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                this.SetSiblings(rosterIdentityKey, rosterStringKey);
            }

            if (rosterId == IdOf.foodConsumption)
            {
                var rosterLevel = new FoodConsumption_type(rosterVector, rosterIdentityKey, parent, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                this.SetSiblings(rosterIdentityKey, rosterStringKey);
            }

            if (rosterId == IdOf.fixedId)
            {
                var rosterLevel = new Education_type(rosterVector, rosterIdentityKey, parent, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
                this.InterviewScopes.Add(rosterStringKey, rosterLevel);
                this.SetSiblings(rosterIdentityKey, rosterStringKey);
            }
        }

        public override void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
            string rosterTitle)
        {
            if (!IdOf.parentScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(IdOf.parentScopeMap[rosterId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            var rosterLevel = this.InterviewScopes[rosterStringKey] as IRosterLevel;
            if (rosterLevel != null)
                rosterLevel.SetRowName(rosterTitle);
        }

        public override void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!IdOf.parentScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(IdOf.parentScopeMap[rosterId], rosterVector);
            var rosterStringKey = Util.GetRosterStringKey(rosterIdentityKey);

            var dependentRostersStringKeys = this.InterviewScopes.Keys.Where(x => x.StartsWith(rosterStringKey)).ToList();
            
            foreach (var rosterKey in dependentRostersStringKeys)
            {
                this.InterviewScopes.Remove(rosterKey);

                foreach (var siblings in this.SiblingRosters.Values)
                {
                    siblings.Remove(rosterKey);
                }
            }
        }

        public override void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
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

        public override void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.price_for_food)
            {
                (targetLevel as FoodConsumption_type).price_for_food = answer;
            }
        }

        public override void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            if (questionId == IdOf.date)
            {
                (targetLevel as HhMember_type).date = answer;
            }
        }

        public override void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;
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

        public override void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
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

        public override void UpdateGeoLocationAnswer(Guid questionId, decimal[] rosterVector, double latitude, double longitude, double accuracy, double altitude)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public override void UpdateTextListAnswer(Guid questionId, decimal[] rosterVector, Tuple<decimal, string>[] answers)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;
        }

        public override void 
            UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] selectedPropagationVector)
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

        public override Dictionary<Guid, Guid[]> GetParentsMap()
        {
            return IdOf.parentScopeMap;
        }

        public override IInterviewExpressionState Clone()
        {
            return new StronglyTypedInterviewEvaluator(this.InterviewScopes, this.SiblingRosters);
        }


        public class QuestionnaireLevel : AbstractConditionalLevel<QuestionnaireLevel>, IExpressionExecutable
        {
            public QuestionnaireLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
                this.EnablementStates.Add(this.id_state.ItemId, this.id_state);
                this.EnablementStates.Add(this.edu_visit_state.ItemId, this.edu_visit_state);
                //EnablementStates.Add(persons_count_state.ItemId, persons_count_state);

                this.QuestionStringUpdateMap.Add(IdOf.id, (s) => {this.__id = s; });
            }

            private string @__id;
            private readonly ConditionalState id_state = new ConditionalState(IdOf.id);
            public string id
            {
                get { return this.id_state.State == State.Enabled ? this.__id : String.Empty; }
                set { this.__id = value; }
            }

            private long? @__persons_count;
            private readonly ConditionalState persons_count_state = new ConditionalState(IdOf.persons_count);
            public long? persons_count
            {
                get { return this.persons_count_state.State == State.Enabled ? this.__persons_count : null; }
                set { this.__persons_count = value; }
            }

            private decimal? @__edu_visit;
            private readonly ConditionalState edu_visit_state = new ConditionalState(IdOf.edu_visit);
            public decimal? edu_visit
            {
                get { return this.edu_visit_state.State == State.Enabled ? this.__edu_visit : null; }
                set { this.__edu_visit = value; }
            }

            public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
            {
                var level = new QuestionnaireLevel(this.RosterVector, this.RosterKey, getInstances, this.ConditionalDependencies, this.StructuralDependencies);

                foreach (var conditionalState in level.EnablementStates)
                {
                    var oldState = this.EnablementStates[conditionalState.Key];
                    conditionalState.Value.State = oldState.State;
                    conditionalState.Value.PreviousState = oldState.PreviousState;
                }

                this.ConditionalDependencies = new Dictionary<Guid, Guid[]>(this.ConditionalDependencies);


                level.id = this.@__id;
                level.persons_count = this.@__persons_count;

                return level;
            }

            private bool edu_visit_IsEnabled()
            {
                return true;
            }

            public void SetParent(IExpressionExecutable parentLevel)
            {
            }

            public IExpressionExecutable GetParent()
            {
                return null;
            }

            public void CalculateValidationChanges(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid)
            {
                questionsToBeValid = new List<Identity>();
                questionsToBeInvalid =new List<Identity>();
            }

            protected override IEnumerable<Action> ConditionExpressions
            {
                get
                {
                    return new[] { this.Verifier(this.edu_visit_IsEnabled, this.edu_visit_state.ItemId, this.edu_visit_state) };
                }
            }

            public RosterRowList<HhMember_type> hhMembers
            {
                get
                {
                    var rosters = this.GetInstances(new Identity[0], IdOf.hhMemberScopeIds.Last());
                    return new RosterRowList<HhMember_type>(rosters);
                }
            }

            public RosterRowList<Education_type> educations
            {
                get
                {
                    var rosters = this.GetInstances(new Identity[0], IdOf.eduScopeIds.Last());
                    return new RosterRowList<Education_type>(rosters);
                }
            }
        }

        //roster first level
        public class HhMember_type : AbstractConditionalLevel<HhMember_type>, IRosterLevel<HhMember_type>, IExpressionExecutable
        {
            public HhMember_type(decimal[] rosterVector, Identity[] rosterKey, IExpressionExecutable parent, Func<Identity[], Guid,
                IEnumerable<IExpressionExecutable>> getInstances, Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
                this.@__parent = parent as QuestionnaireLevel;
            }

            public HhMember_type(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
                this.ValidationExpressions.Add(new Identity(IdOf.name, rosterVector), new Func<bool>[] { this.name_IsMandatory });
                this.ValidationExpressions.Add(new Identity(IdOf.age, rosterVector), new Func<bool>[] { this.age_IsValid });
                this.ValidationExpressions.Add(new Identity(IdOf.food, rosterVector), new Func<bool>[] { this.food_IsValid });
                this.ValidationExpressions.Add(new Identity(IdOf.role, rosterVector), new Func<bool>[] { this.role_IsValid, this.role2_IsValid });
                this.ValidationExpressions.Add(new Identity(IdOf.married_with, rosterVector), new Func<bool>[] { this.married_with_IsValid });

                this.EnablementStates.Add(this.age_state.ItemId, this.age_state);
                this.EnablementStates.Add(this.married_with_state.ItemId, this.married_with_state);
                this.EnablementStates.Add(this.has_job_state.ItemId, this.has_job_state);
                this.EnablementStates.Add(this.job_title_state.ItemId, this.job_title_state);
                this.EnablementStates.Add(this.best_job_owner_state.ItemId, this.best_job_owner_state);
                this.EnablementStates.Add(this.food_state.ItemId, this.food_state);
                this.EnablementStates.Add(this.person_id_state.ItemId, this.person_id_state);
                this.EnablementStates.Add(this.marital_status_state.ItemId, this.marital_status_state);
                this.EnablementStates.Add(this.group_state.ItemId, this.group_state);


                this.QuestionLongUpdateMap.Add(IdOf.age, l => { this.__age = l; });

            }

            private QuestionnaireLevel @__parent;

            public IEnumerable<HhMember_type> hhMembers
            {
                get { return this.__parent.hhMembers; }
            }

            public RosterRowList<FoodConsumption_type> foodConsumption
            {
                get
                {
                    var rosters = this.GetInstances(this.RosterKey, IdOf.foodConsumptionIds.Last());
                    return new RosterRowList<FoodConsumption_type>(rosters);
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
                get { return this.person_id_state.State == State.Enabled ? this.@__personId : null; }
                set { this.@__personId = value; }
            }

            public decimal? marital_status
            {
                get { return this.marital_status_state.State == State.Enabled ? this.@__maritalStatus : null; }
                set { this.@__maritalStatus = value; }
            }

            public decimal[][] married_with
            {
                get { return this.married_with_state.State == State.Enabled ? this.@__marriedWith : null; }
                set { this.@__marriedWith = value; }
            }

            public long? age
            {
                get { return this.age_state.State != State.Disabled ? this.@__age : null; }
                set { this.@__age = value; }
            }

            public decimal[] food
            {
                get { return this.food_state.State == State.Enabled ? this.@__food : null; }
                set { this.@__food = value; }
            }

            public decimal? has_job
            {
                get { return this.has_job_state.State == State.Enabled ? this.@__hasJob : null; }
                set { this.@__hasJob = value; }
            }

            public string job_title
            {
                get { return this.job_title_state.State == State.Enabled ? this.@__jobTitle : null; }
                set { this.@__jobTitle = value; }
            }

            public decimal[] best_job_owner
            {
                get { return this.best_job_owner_state.State == State.Enabled ? this.@__bestJobOwner : null; }
                set { this.@__bestJobOwner = value; }
            }

            private ConditionalState age_state = new ConditionalState(IdOf.age);
            private ConditionalState married_with_state = new ConditionalState(IdOf.married_with);
            private ConditionalState has_job_state = new ConditionalState(IdOf.has_job);
            private ConditionalState job_title_state = new ConditionalState(IdOf.job_title);
            private ConditionalState best_job_owner_state = new ConditionalState(IdOf.best_job_owner);
            private ConditionalState food_state = new ConditionalState(IdOf.food);
            private ConditionalState person_id_state = new ConditionalState(IdOf.person_id);
            private ConditionalState marital_status_state = new ConditionalState(IdOf.marital_status);

            private ConditionalState group_state = new ConditionalState(IdOf.groupId, ItemType.Group);

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
                    this.Verifier(this.age_IsEnabledIf, this.age_state.ItemId, this.age_state),
                    this.Verifier(this.group_IsEnabledIf, this.group_state.ItemId, this.group_state),
                    this.Verifier(this.IsEnabledIfParentIs, this.person_id_state.ItemId, this.person_id_state),
                    this.Verifier(this.IsEnabledIfParentIs, this.marital_status_state.ItemId, this.marital_status_state),
                    this.Verifier(this.married_with_IsEnabledIf, this.married_with_state.ItemId, this.married_with_state),
                    this.Verifier(this.food_IsEnabledIf, this.food_state.ItemId, this.food_state),
                    this.Verifier(this.has_job_IsEnabledIf, this.has_job_state.ItemId, this.has_job_state),
                    this.Verifier(this.job_title_IsEnabledIf, this.job_title_state.ItemId, this.job_title_state),
                    this.Verifier(this.best_job_owner_IsEnabledIf, this.best_job_owner_state.ItemId, this.best_job_owner_state)
                };
                }
            }

            private bool age_IsEnabledIf()
            {
                return this.name.ToLower().StartsWith("a");
            }

            private bool group_IsEnabledIf()
            {
                return (this.age > 16);
            }

            private bool married_with_IsEnabledIf()
            {
                return this.marital_status == 2 && this.persons_count > 1;
            }

            private bool food_IsEnabledIf()
            {
                return this.role == 2 && this.sex == 2;
            }

            private bool has_job_IsEnabledIf()
            {
                return this.age > 16;
            }

            private bool job_title_IsEnabledIf()
            {
                return this.has_job == 1;
            }

            private bool best_job_owner_IsEnabledIf()
            {
                return this.has_job == 2;
            }

            private bool age_IsValid()
            {
                return this.age >= 0 && this.age < 100;
            }

            private bool married_with_IsValid()
            {
                return !this.married_with.Any(x => x.SequenceEqual(this.me));
            }

            private bool name_IsMandatory()
            {
                return !this.IsAnswerEmpty(this.name);
            }

            private bool food_IsValid()
            {
                return this.food == null || !(this.food.Contains(38) && this.role == 3 && this.age >= 21);
            }

            private bool role_IsValid()
            {
                // children should not drink alcohol
                return (this.role == 1 && this.hhMembers.Count(x => x.role == 1) == 1) || this.role != 1;
            }

            private bool role2_IsValid()
            {
                // children should not drink alcohol
                return (this.role == 3 && this.hhMembers.Where(x => x.role < 3).Any(x => x.age < this.age + 10)) || this.role != 3;
            }

            public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
            {
                var level = new HhMember_type(this.RosterVector, this.RosterKey, getInstances, this.ConditionalDependencies, this.StructuralDependencies)
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
                ((IRosterLevel)level).SetRowName(this.@rowname);
                foreach (var state in level.EnablementStates)
                {
                    var originalState = this.EnablementStates[state.Key];
                    state.Value.PreviousState = originalState.PreviousState;
                    state.Value.State = originalState.State;
                }

                return level;
            }

            public void SetParent(IExpressionExecutable parentLevel)
            {
                this.@__parent = parentLevel as QuestionnaireLevel;
            }

            public IExpressionExecutable GetParent()
            {
                return this.@__parent;
            }

            public void CalculateValidationChanges(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid)
            {
                this.Validate(out questionsToBeValid,out questionsToBeInvalid);
            }

            public decimal @rowcode
            {
                get { return RosterVector.Last(); }
            }

            public string @rowname { get; private set; }

            void IRosterLevel.SetRowName(string rosterRowName)
            {
                this.@rowname = rosterRowName;
            }

            public int @rowindex
            {
                get { return (@__parent).hhMembers.Select((s, i) => new { Index = i, Value = s }).Where(t => t.Value.@rowcode == this.@rowcode).Select(t => t.Index).First(); }
            }

            public RosterRowList<HhMember_type> @roster
            {
                get { return @__parent.hhMembers; }
            }
        }

        //roster second level
        public class FoodConsumption_type : AbstractConditionalLevel<FoodConsumption_type>, IRosterLevel<FoodConsumption_type>, IExpressionExecutable
        {
            public FoodConsumption_type(decimal[] rosterVector, Identity[] rosterKey, IExpressionExecutable parent, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
                this.@__parent = parent as HhMember_type;
            }

            public FoodConsumption_type(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
                this.ValidationExpressions.Add(new Identity(IdOf.times_per_week, rosterVector), new Func<bool>[] { this.times_per_week_validation });

                this.EnablementStates.Add(this.price_for_food_state.ItemId, this.price_for_food_state);
            }

            private HhMember_type @__parent;

            public IEnumerable<HhMember_type> hhMembers
            {
                get { return this.__parent.hhMembers; }
            }

            public IEnumerable<FoodConsumption_type> foodConsumption
            {
                get { return this.__parent.foodConsumption; }
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

            public long? times_per_week { get; set; }

            private bool times_per_week_validation()
            {
                return this.times_per_week > 0 && this.times_per_week < 7 * 5;
            }

            public double? price_for_food
            {
                get { return this.price_for_food_state.State == State.Enabled ? this.priceForFood : null; }
                set { this.priceForFood = value; }
            }

            private ConditionalState price_for_food_state = new ConditionalState(IdOf.price_for_food);
            private double? priceForFood;

            private bool price_for_food_IsEnabledIf()
            {
                return this.times_per_week > 0;
            }

            public void CalculateValidationChanges(out List<Identity> questionsToBeValid,out List<Identity> questionsToBeInvalid)
            {
                this.Validate(out questionsToBeValid, out questionsToBeInvalid);
            }

            public decimal @rowcode
            {
                get { return RosterVector.Last(); }
            }

            public string @rowname { get; private set; }

            public void SetRowName(string rosterRowName)
            {
                this.@rowname = rosterRowName;
            }

            public int @rowindex
            {
                get { return (@__parent).hhMembers.Select((s, i) => new { Index = i, Value = s }).Where(t => t.Value.@rowcode == this.@rowcode).Select(t => t.Index).First(); }
            }

            public RosterRowList<FoodConsumption_type> @roster
            {
                get { return @__parent.foodConsumption; }
            }

            public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
            {
                var level = new FoodConsumption_type(this.RosterVector, this.RosterKey, getInstances, this.ConditionalDependencies, this.StructuralDependencies)
                {
                    ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                    InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),


                    price_for_food = this.priceForFood,
                    times_per_week = this.times_per_week
                };
                level.SetRowName(this.@rowname);
                foreach (var state in level.EnablementStates)
                {
                    var originalState = this.EnablementStates[state.Key];
                    state.Value.PreviousState = originalState.PreviousState;
                    state.Value.State = originalState.State;
                }

                return level;
            }

            public void SetParent(IExpressionExecutable parentLevel)
            {
                this.@__parent = parentLevel as HhMember_type;
            }

            public IExpressionExecutable GetParent()
            {
                return this.@__parent;
            }

            protected override IEnumerable<Action> ConditionExpressions
            {
                get
                {
                    return new[]
                {
                    this.Verifier(this.price_for_food_IsEnabledIf,this.price_for_food_state.ItemId, this.price_for_food_state)
                };
                }
            }
        }

        public class Education_type : AbstractConditionalLevel<Education_type>, IRosterLevel<Education_type>, IExpressionExecutable
        {
            public Education_type(decimal[] rosterVector, Identity[] rosterKey, IExpressionExecutable parent, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
                this.@__parent = parent as QuestionnaireLevel;
            }

            public Education_type(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
                : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
            {
            }

            private QuestionnaireLevel @__parent;

            public IEnumerable<Education_type> educations
            {
                get { return this.__parent.educations; }
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

            public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
            {
                var level = new Education_type(this.RosterVector, this.RosterKey, getInstances, this.ConditionalDependencies, this.StructuralDependencies)
                {
                    ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                    InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),

                    edu = this.edu
                };
                level.SetRowName(this.@rowname);
                foreach (var state in level.EnablementStates)
                {
                    var originalState = this.EnablementStates[state.Key];
                    state.Value.PreviousState = originalState.PreviousState;
                    state.Value.State = originalState.State;
                }

                return level;
            }

            public void SetParent(IExpressionExecutable parentLevel)
            {
                this.@__parent = parentLevel as QuestionnaireLevel;
            }

            public IExpressionExecutable GetParent()
            {
                return this.@__parent;
            }

            public void CalculateValidationChanges(out List<Identity> questionsToBeValid,out List<Identity> questionsToBeInvalid)
            {
                this.Validate(out questionsToBeValid, out questionsToBeInvalid);
            }

            public decimal @rowcode
            {
                get { return RosterVector.Last(); }
            }

            public string @rowname { get; private set; }

            public void SetRowName(string rosterRowName)
            {
                this.@rowname = rosterRowName;
            }

            public int @rowindex
            {
                get { return (@__parent).hhMembers.Select((s, i) => new { Index = i, Value = s }).Where(t => t.Value.@rowcode == this.@rowcode).Select(t => t.Index).First(); }
            }

            public RosterRowList<Education_type> @roster
            {
                get { return @__parent.educations; }
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

            public static Dictionary<Guid, Guid[]> structuralDependencies = new Dictionary<Guid, Guid[]>()
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

        public static Dictionary<Guid, Guid[]> parentScopeMap = new Dictionary<Guid, Guid[]>
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
            { jobActivity, hhMemberScopeIds }
            
        };

        /*    public static Dictionary<Guid, Guid[]> rostersIdToScopeMap = new Dictionary<Guid, Guid[]>
        {
            { fixedId, eduScopeIds },
            { hhMember, hhMemberScopeIds },
            { foodConsumption, foodConsumptionIds },
            { jobActivity, hhMemberScopeIds }
        };*/
        }
    }
}

// ReSharper restore InconsistentNaming

