using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace WB.Core.SharedKernels.DataCollection.Generated
{
    public class InterviewExpressionState_14db7def8da4439db323184475aa64e4 : AbstractInterviewExpressionState
    {
        public InterviewExpressionState_14db7def8da4439db323184475aa64e4()
        {
            var questionnaireLevelScope = new[] { IdOf.@__questionnaire };
            var questionnaireIdentityKey = Util.GetRosterKey(questionnaireLevelScope, Util.EmptyRosterVector);
            var questionnaireLevel = new QuestionnaireTopLevel(Util.EmptyRosterVector, questionnaireIdentityKey, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
            this.InterviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
        }

        private InterviewExpressionState_14db7def8da4439db323184475aa64e4(Dictionary<string, IExpressionExecutable> interviewScopes, Dictionary<string, List<string>> siblingRosters)
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
                ? Util.GetRosterKey(new[] { IdOf.@__questionnaire }, new decimal[0])
                : Util.GetRosterKey(rosterScopeIds.Shrink(), outerRosterVector);

            var parent = this.InterviewScopes[Util.GetRosterStringKey(rosterParentIdentityKey)];

            var rosterLevel = parent.CreateChildRosterInstance(rosterId, rosterVector, rosterIdentityKey);

            this.InterviewScopes.Add(rosterStringKey, rosterLevel);
            this.SetSiblings(rosterIdentityKey, rosterStringKey);
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
            this.InterviewScopes[rosterStringKey].SetTitle(rosterTitle);
        }

        public override void RemoveRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            if (!IdOf.parentScopeMap.ContainsKey(rosterId))
            {
                return;
            }

            decimal[] rosterVector = Util.GetRosterVector(outerRosterVector, rosterInstanceId);
            var rosterIdentityKey = Util.GetRosterKey(IdOf.parentScopeMap[rosterId], rosterVector);

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

        public override void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateNumericIntegerAnswer(questionId, answer);
        }

        public override void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateNumericRealAnswer(questionId, answer);
        }

        public override void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateDateTimeAnswer(questionId, answer);
        }

        public override void UpdateMediaAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateMediaAnswer(questionId, answer);
        }

        public override void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateTextAnswer(questionId, answer);
        }

        public override void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateQrBarcodeAnswer(questionId, answer);
        }

        public override void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal? answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateSingleOptionAnswer(questionId, answer);
        }

        public override void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateMultiOptionAnswer(questionId, answer);
        }

        public override void UpdateGeoLocationAnswer(Guid questionId, decimal[] rosterVector, double latitude, double longitude, double accuracy, double altitude)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateGeoLocationAnswer(questionId, latitude, longitude, accuracy, altitude);
        }

        public override void UpdateTextListAnswer(Guid questionId, decimal[] rosterVector, Tuple<decimal, string>[] answers)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateTextListAnswer(questionId, answers);
        }

        public override void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] selectedPropagationVector)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateLinkedSingleOptionAnswer(questionId, selectedPropagationVector);
        }

        public override void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[][] answer)
        {
            var targetLevel = this.GetRosterByIdAndVector(questionId, rosterVector);
            if (targetLevel == null) return;

            targetLevel.UpdateLinkedMultiOptionAnswer(questionId, answer);
        }

        public override Dictionary<Guid, Guid[]> GetParentsMap()
        {
            return IdOf.parentScopeMap;
        }

        public override IInterviewExpressionState Clone()
        {
            return new InterviewExpressionState_14db7def8da4439db323184475aa64e4(this.InterviewScopes, this.SiblingRosters);
        }

    }


    //generate QuestionnaireLevel

    internal partial class QuestionnaireTopLevel : AbstractConditionalLevel<QuestionnaireTopLevel>, IExpressionExecutable
    {
        public QuestionnaireTopLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {

            EnablementStates.Add(@__d48b99970efd476f94ed81be31c999ee_state.ItemId, @__d48b99970efd476f94ed81be31c999ee_state);

            EnablementStates.Add(@__txt_state.ItemId, @__txt_state);
            AddUpdaterToMap(IdOf.@__txt_id, (string val) => { @__txt = val; });
            EnablementStates.Add(@__num_int_state.ItemId, @__num_int_state);
            AddUpdaterToMap(IdOf.@__num_int_id, (long? val) => { @__num_int = val; });
            EnablementStates.Add(@__num_real_state.ItemId, @__num_real_state);
            AddUpdaterToMap(IdOf.@__num_real_id, (double? val) => { @__num_real = val; });
            EnablementStates.Add(@__geo_state.ItemId, @__geo_state);
            AddUpdaterToMap(IdOf.@__geo_id, (GeoLocation val) => { @__geo = val; });
            EnablementStates.Add(@__date_state.ItemId, @__date_state);
            AddUpdaterToMap(IdOf.@__date_id, (DateTime? val) => { @__date = val; });
            EnablementStates.Add(@__list_state.ItemId, @__list_state);
            AddUpdaterToMap(IdOf.@__list_id, (Tuple<decimal, string>[] val) => { @__list = val; });
            EnablementStates.Add(@__qr_barcode_state.ItemId, @__qr_barcode_state);
            AddUpdaterToMap(IdOf.@__qr_barcode_id, (string val) => { @__qr_barcode = val; });
            EnablementStates.Add(@__multimedia_state.ItemId, @__multimedia_state);
            AddUpdaterToMap(IdOf.@__multimedia_id, (string val) => { @__multimedia = val; });
            EnablementStates.Add(@__single_option_state.ItemId, @__single_option_state);
            AddUpdaterToMap(IdOf.@__single_option_id, (decimal? val) => { @__single_option = val; });
            EnablementStates.Add(@__muti_option_state.ItemId, @__muti_option_state);
            AddUpdaterToMap(IdOf.@__muti_option_id, (decimal[] val) => { @__muti_option = val; });
            EnablementStates.Add(@__single_option_linked_state.ItemId, @__single_option_linked_state);
            AddUpdaterToMap(IdOf.@__single_option_linked_id, (decimal[] val) => { @__single_option_linked = val; });
            EnablementStates.Add(@__multi_option_linked_state.ItemId, @__multi_option_linked_state);
            AddUpdaterToMap(IdOf.@__multi_option_linked_id, (decimal[][] val) => { @__multi_option_linked = val; });
            EnablementStates.Add(@__mandatory_text_state.ItemId, @__mandatory_text_state);

            ValidationExpressions.Add(new Identity(IdOf.@__mandatory_text_id, rosterVector), new Func<bool>[] {                             
                  
                IsManadatoryValid_mandatory_text ,                                               
                                            });
            AddUpdaterToMap(IdOf.@__mandatory_text_id, (string val) => { @__mandatory_text = val; });
            EnablementStates.Add(@__example_of_validation_state.ItemId, @__example_of_validation_state);

            ValidationExpressions.Add(new Identity(IdOf.@__example_of_validation_id, rosterVector), new Func<bool>[] {                             
                                  
                    () => this.IsAnswerEmpty(example_of_validation) || IsValid_example_of_validation() , 
                            });
            AddUpdaterToMap(IdOf.@__example_of_validation_id, (string val) => { @__example_of_validation = val; });
            EnablementStates.Add(@__example_of_condition_state.ItemId, @__example_of_condition_state);
            AddUpdaterToMap(IdOf.@__example_of_condition_id, (string val) => { @__example_of_condition = val; });

            RosterGenerators.Add(IdOf.@__fixed_roster_id, (decimals, identities) => new @__f86288c0b1b54426924afb852311e577(decimals, identities, this, this.GetInstances, this.ConditionalDependencies, this.StructuralDependencies));

            _conditionExpressions.Add(Verifier(IsEnabled_example_of_condition, @__example_of_condition_state.ItemId, @__example_of_condition_state));

        }

        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            var level = new QuestionnaireTopLevel(this.RosterVector, this.RosterKey, getInstances, ConditionalDependencies, StructuralDependencies)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),

                @__txt = this.@__txt,

                @__num_int = this.@__num_int,

                @__num_real = this.@__num_real,

                @__geo = this.@__geo,

                @__date = this.@__date,

                @__list = this.@__list,

                @__qr_barcode = this.@__qr_barcode,

                @__multimedia = this.@__multimedia,

                @__single_option = this.@__single_option,

                @__muti_option = this.@__muti_option,

                @__single_option_linked = this.@__single_option_linked,

                @__multi_option_linked = this.@__multi_option_linked,

                @__mandatory_text = this.@__mandatory_text,

                @__example_of_validation = this.@__example_of_validation,

                @__example_of_condition = this.@__example_of_condition,
            };

            ConditionalDependencies = new Dictionary<Guid, Guid[]>(this.ConditionalDependencies);
            StructuralDependencies = new Dictionary<Guid, Guid[]>(this.StructuralDependencies);

            foreach (var state in level.EnablementStates)
            {
                var originalState = this.EnablementStates[state.Key];
                state.Value.PreviousState = originalState.PreviousState;
                state.Value.State = originalState.State;
            }

            return level;
        }


        private string @__txt = null;
        private ConditionalState @__txt_state = new ConditionalState(IdOf.@__txt_id);
        public string txt
        {
            get { return @__txt_state.State != State.Disabled ? this.@__txt : null; }

        }



        private long? @__num_int = null;
        private ConditionalState @__num_int_state = new ConditionalState(IdOf.@__num_int_id);
        public long? num_int
        {
            get { return @__num_int_state.State != State.Disabled ? this.@__num_int : null; }

        }



        private double? @__num_real = null;
        private ConditionalState @__num_real_state = new ConditionalState(IdOf.@__num_real_id);
        public double? num_real
        {
            get { return @__num_real_state.State != State.Disabled ? this.@__num_real : null; }

        }



        private GeoLocation @__geo = null;
        private ConditionalState @__geo_state = new ConditionalState(IdOf.@__geo_id);
        public GeoLocation geo
        {
            get { return @__geo_state.State != State.Disabled ? this.@__geo : null; }

        }



        private DateTime? @__date = null;
        private ConditionalState @__date_state = new ConditionalState(IdOf.@__date_id);
        public DateTime? date
        {
            get { return @__date_state.State != State.Disabled ? this.@__date : null; }

        }



        private Tuple<decimal, string>[] @__list = null;
        private ConditionalState @__list_state = new ConditionalState(IdOf.@__list_id);
        public Tuple<decimal, string>[] list
        {
            get { return @__list_state.State != State.Disabled ? this.@__list : null; }

        }



        private string @__qr_barcode = null;
        private ConditionalState @__qr_barcode_state = new ConditionalState(IdOf.@__qr_barcode_id);
        public string qr_barcode
        {
            get { return @__qr_barcode_state.State != State.Disabled ? this.@__qr_barcode : null; }

        }



        private string @__multimedia = null;
        private ConditionalState @__multimedia_state = new ConditionalState(IdOf.@__multimedia_id);
        public string multimedia
        {
            get { return @__multimedia_state.State != State.Disabled ? this.@__multimedia : null; }

        }



        private decimal? @__single_option = null;
        private ConditionalState @__single_option_state = new ConditionalState(IdOf.@__single_option_id);
        public decimal? single_option
        {
            get { return @__single_option_state.State != State.Disabled ? this.@__single_option : null; }

        }



        private decimal[] @__muti_option = null;
        private ConditionalState @__muti_option_state = new ConditionalState(IdOf.@__muti_option_id);
        public decimal[] muti_option
        {
            get { return @__muti_option_state.State != State.Disabled ? this.@__muti_option : null; }

        }



        private decimal[] @__single_option_linked = null;
        private ConditionalState @__single_option_linked_state = new ConditionalState(IdOf.@__single_option_linked_id);
        public decimal[] single_option_linked
        {
            get { return @__single_option_linked_state.State != State.Disabled ? this.@__single_option_linked : null; }

        }



        private decimal[][] @__multi_option_linked = null;
        private ConditionalState @__multi_option_linked_state = new ConditionalState(IdOf.@__multi_option_linked_id);
        public decimal[][] multi_option_linked
        {
            get { return @__multi_option_linked_state.State != State.Disabled ? this.@__multi_option_linked : null; }

        }



        private string @__mandatory_text = null;
        private ConditionalState @__mandatory_text_state = new ConditionalState(IdOf.@__mandatory_text_id);
        public string mandatory_text
        {
            get { return @__mandatory_text_state.State != State.Disabled ? this.@__mandatory_text : null; }

        }

        private bool IsManadatoryValid_mandatory_text()
        {
            return !this.IsAnswerEmpty(mandatory_text);
        }



        private string @__example_of_validation = null;
        private ConditionalState @__example_of_validation_state = new ConditionalState(IdOf.@__example_of_validation_id);
        public string example_of_validation
        {
            get { return @__example_of_validation_state.State != State.Disabled ? this.@__example_of_validation : null; }

        }


        private bool IsValid_example_of_validation()
        {
            return num_int > 10;
        }


        private string @__example_of_condition = null;
        private ConditionalState @__example_of_condition_state = new ConditionalState(IdOf.@__example_of_condition_id);
        public string example_of_condition
        {
            get { return @__example_of_condition_state.State != State.Disabled ? this.@__example_of_condition : null; }

        }
        private bool IsEnabled_example_of_condition()
        {
            return num_real == 10 && fixed_roster.Any(x => x.text_inside_roster == "Sergey");
        }



        public IList<@__f86288c0b1b54426924afb852311e577> fixed_roster
        {
            get
            {
                var rosters = this.GetInstances(new Identity[0], IdOf.@__fixed_roster_scope.Last());
                return rosters == null ? new List<@__f86288c0b1b54426924afb852311e577>() : rosters.Select(x => x as @__f86288c0b1b54426924afb852311e577).ToList();
            }
        }
        // groups condition states
        private ConditionalState @__d48b99970efd476f94ed81be31c999ee_state = new ConditionalState(IdOf.@__d48b99970efd476f94ed81be31c999ee_id, ItemType.Group);

        private readonly List<Action> _conditionExpressions = new List<Action>();

        protected override IEnumerable<Action> ConditionExpressions
        {
            get
            {
                return _conditionExpressions;
            }
        }

        public void CalculateValidationChanges(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid)
        {
            this.Validate(out questionsToBeValid, out questionsToBeInvalid);
        }

        public void SetParent(IExpressionExecutable parentLevel)
        {
        }

        public IExpressionExecutable GetParent()
        {
            return null;
        }
    }

    //generating rosters
    internal partial class @__f86288c0b1b54426924afb852311e577 : AbstractConditionalLevel<@__f86288c0b1b54426924afb852311e577>, IExpressionExecutable
    {
        public @__f86288c0b1b54426924afb852311e577(decimal[] rosterVector, Identity[] rosterKey, QuestionnaireTopLevel parent, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {
            this.@_parent = parent;
        }

        private @__f86288c0b1b54426924afb852311e577(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {

            EnablementStates.Add(@__fixed_roster_state.ItemId, @__fixed_roster_state);


            EnablementStates.Add(@__text_inside_roster_state.ItemId, @__text_inside_roster_state);

            ValidationExpressions.Add(new Identity(IdOf.@__text_inside_roster_id, rosterVector), new Func<bool>[] {                             
                                  
                    () => this.IsAnswerEmpty(text_inside_roster) || IsValid_text_inside_roster() , 
                            });
            AddUpdaterToMap(IdOf.@__text_inside_roster_id, (string val) => { @__text_inside_roster = val; });


        }

        private QuestionnaireTopLevel @_parent;

        public string txt { get { return this.@_parent.txt; } }
        public long? num_int { get { return this.@_parent.num_int; } }
        public double? num_real { get { return this.@_parent.num_real; } }
        public GeoLocation geo { get { return this.@_parent.geo; } }
        public DateTime? date { get { return this.@_parent.date; } }
        public Tuple<decimal, string>[] list { get { return this.@_parent.list; } }
        public string qr_barcode { get { return this.@_parent.qr_barcode; } }
        public string multimedia { get { return this.@_parent.multimedia; } }
        public decimal? single_option { get { return this.@_parent.single_option; } }
        public decimal[] muti_option { get { return this.@_parent.muti_option; } }
        public decimal[] single_option_linked { get { return this.@_parent.single_option_linked; } }
        public decimal[][] multi_option_linked { get { return this.@_parent.multi_option_linked; } }
        public string mandatory_text { get { return this.@_parent.mandatory_text; } }
        public string example_of_validation { get { return this.@_parent.example_of_validation; } }
        public string example_of_condition { get { return this.@_parent.example_of_condition; } }

        public IList<@__f86288c0b1b54426924afb852311e577> fixed_roster { get { return this.@_parent.fixed_roster; } }

        public object @rosterValue { get { return RosterVector.Last(); } }
        public int @index { get { return @_parent.fixed_roster.Select((s, i) => new { Index = i, Value = s }).Where(t => t.Value.@rosterValue == this.@rosterValue).Select(t => t.Index).First(); } }


        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            var level = new @__f86288c0b1b54426924afb852311e577(this.RosterVector, this.RosterKey, getInstances, ConditionalDependencies, StructuralDependencies)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),


                @__text_inside_roster = this.@__text_inside_roster,
            };

            ConditionalDependencies = new Dictionary<Guid, Guid[]>(this.ConditionalDependencies);
            StructuralDependencies = new Dictionary<Guid, Guid[]>(this.StructuralDependencies);

            foreach (var state in level.EnablementStates)
            {
                var originalState = this.EnablementStates[state.Key];
                state.Value.PreviousState = originalState.PreviousState;
                state.Value.State = originalState.State;
            }

            return level;
        }



        private string @__text_inside_roster = null;
        private ConditionalState @__text_inside_roster_state = new ConditionalState(IdOf.@__text_inside_roster_id);
        public string text_inside_roster
        {
            get { return @__text_inside_roster_state.State != State.Disabled ? this.@__text_inside_roster : null; }

        }


        private bool IsValid_text_inside_roster()
        {
            return @title == "c" && @rosterValue == 2 && @index == 2;
        }
        //groups
        //rosters



        private ConditionalState @__fixed_roster_state = new ConditionalState(IdOf.@__fixed_roster_id, ItemType.Group);




        private readonly List<Action> _conditionExpressions = new List<Action>();

        protected override IEnumerable<Action> ConditionExpressions
        {
            get
            {
                return _conditionExpressions;
            }
        }

        public void CalculateValidationChanges(out List<Identity> questionsToBeValid, out List<Identity> questionsToBeInvalid)
        {
            this.Validate(out questionsToBeValid, out questionsToBeInvalid);
        }

        public void SetParent(IExpressionExecutable parentLevel)
        {
            this.@_parent = parentLevel as QuestionnaireTopLevel;
        }

        public IExpressionExecutable GetParent()
        {
            return this.@_parent;
        }
    }

    public static class IdOf
    {
        public static readonly Guid @__questionnaire = Guid.Parse("c898da1c-c3e8-4667-8db9-5ed5ba4efac8");
        //questions
        public static readonly Guid @__txt_id = Guid.Parse("b643d74d-c33a-fa27-5c77-89f4184844e4");
        public static readonly Guid @__num_int_id = Guid.Parse("f8a64e47-50ea-d615-d148-d7d53519e21f");
        public static readonly Guid @__num_real_id = Guid.Parse("b1ed800b-37ad-5d90-a331-9ecbea1f3f01");
        public static readonly Guid @__geo_id = Guid.Parse("98d787b7-84ae-bc1a-13ac-afa86f99fa11");
        public static readonly Guid @__date_id = Guid.Parse("38019b3a-9e54-2acc-5737-8f6826ae5a38");
        public static readonly Guid @__list_id = Guid.Parse("1eb4822a-727e-c9f3-57e9-b3ad9a85f494");
        public static readonly Guid @__qr_barcode_id = Guid.Parse("ba1a5675-5779-c3c7-a192-8e9801c33b05");
        public static readonly Guid @__multimedia_id = Guid.Parse("baa28dd8-3628-5c36-721a-3c8adeb0e047");
        public static readonly Guid @__single_option_id = Guid.Parse("05c5965a-3842-61b9-a2f8-f17217d1a43b");
        public static readonly Guid @__muti_option_id = Guid.Parse("417953b9-a181-c276-ff22-221b4fcbd49c");
        public static readonly Guid @__single_option_linked_id = Guid.Parse("94616f19-bb90-1670-77ba-5418d3405ca2");
        public static readonly Guid @__multi_option_linked_id = Guid.Parse("53baef68-65b8-46ed-63d2-63bf318893e1");
        public static readonly Guid @__mandatory_text_id = Guid.Parse("1e4e8233-ec40-2751-4bcd-aae7ae1ca70c");
        public static readonly Guid @__example_of_validation_id = Guid.Parse("0e1642ee-da24-a5e1-08b1-4fb1a672c6e8");
        public static readonly Guid @__example_of_condition_id = Guid.Parse("157acd25-49ce-42f4-9126-d4bfcc00e287");
        public static readonly Guid @__text_inside_roster_id = Guid.Parse("b53abb47-145d-617c-d0e2-a7ed2f4d8f49");
        //groups
        public static readonly Guid @__d48b99970efd476f94ed81be31c999ee_id = Guid.Parse("d48b9997-0efd-476f-94ed-81be31c999ee");
        //rosters
        public static readonly Guid @__fixed_roster_id = Guid.Parse("df540d18-6dc1-f288-84a1-45e757bf014d");

        public static readonly Guid[] @__questionnaire_scope = new[] { @__questionnaire };

        public static readonly Guid[] @__fixed_roster_scope = new[] { Guid.Parse("df540d18-6dc1-f288-84a1-45e757bf014d") };

        public static Dictionary<Guid, Guid[]> conditionalDependencies = new Dictionary<Guid, Guid[]>()
            {            
                                {Guid.Parse("157acd25-49ce-42f4-9126-d4bfcc00e287"), new Guid[]{
                                    Guid.Parse("b53abb47-145d-617c-d0e2-a7ed2f4d8f49"),
                                    Guid.Parse("c898da1c-c3e8-4667-8db9-5ed5ba4efac8"),
                                    Guid.Parse("b1ed800b-37ad-5d90-a331-9ecbea1f3f01"),
                                }},
                            };

        public static Dictionary<Guid, Guid[]> structuralDependencies = new Dictionary<Guid, Guid[]>()
            {
                                { Guid.Parse("d48b9997-0efd-476f-94ed-81be31c999ee"), new Guid[]{
                                    Guid.Parse("b643d74d-c33a-fa27-5c77-89f4184844e4"),
                                    Guid.Parse("f8a64e47-50ea-d615-d148-d7d53519e21f"),
                                    Guid.Parse("b1ed800b-37ad-5d90-a331-9ecbea1f3f01"),
                                    Guid.Parse("98d787b7-84ae-bc1a-13ac-afa86f99fa11"),
                                    Guid.Parse("38019b3a-9e54-2acc-5737-8f6826ae5a38"),
                                    Guid.Parse("1eb4822a-727e-c9f3-57e9-b3ad9a85f494"),
                                    Guid.Parse("ba1a5675-5779-c3c7-a192-8e9801c33b05"),
                                    Guid.Parse("baa28dd8-3628-5c36-721a-3c8adeb0e047"),
                                    Guid.Parse("05c5965a-3842-61b9-a2f8-f17217d1a43b"),
                                    Guid.Parse("417953b9-a181-c276-ff22-221b4fcbd49c"),
                                    Guid.Parse("df540d18-6dc1-f288-84a1-45e757bf014d"),
                                    Guid.Parse("94616f19-bb90-1670-77ba-5418d3405ca2"),
                                    Guid.Parse("53baef68-65b8-46ed-63d2-63bf318893e1"),
                                    Guid.Parse("1e4e8233-ec40-2751-4bcd-aae7ae1ca70c"),
                                    Guid.Parse("0e1642ee-da24-a5e1-08b1-4fb1a672c6e8"),
                                    Guid.Parse("157acd25-49ce-42f4-9126-d4bfcc00e287"),
                                }},
                                { Guid.Parse("df540d18-6dc1-f288-84a1-45e757bf014d"), new Guid[]{
                                    Guid.Parse("b53abb47-145d-617c-d0e2-a7ed2f4d8f49"),
                                }},
                            };

        public static Dictionary<Guid, Guid[]> parentScopeMap = new Dictionary<Guid, Guid[]>
            {
                //questions
                                {@__txt_id, @__questionnaire_scope},
                                {@__num_int_id, @__questionnaire_scope},
                                {@__num_real_id, @__questionnaire_scope},
                                {@__geo_id, @__questionnaire_scope},
                                {@__date_id, @__questionnaire_scope},
                                {@__list_id, @__questionnaire_scope},
                                {@__qr_barcode_id, @__questionnaire_scope},
                                {@__multimedia_id, @__questionnaire_scope},
                                {@__single_option_id, @__questionnaire_scope},
                                {@__muti_option_id, @__questionnaire_scope},
                                {@__single_option_linked_id, @__questionnaire_scope},
                                {@__multi_option_linked_id, @__questionnaire_scope},
                                {@__mandatory_text_id, @__questionnaire_scope},
                                {@__example_of_validation_id, @__questionnaire_scope},
                                {@__example_of_condition_id, @__questionnaire_scope},
                                {@__text_inside_roster_id, @__fixed_roster_scope},
                                //groups
                                {@__d48b99970efd476f94ed81be31c999ee_id, @__questionnaire_scope},
                                //rosters
                                {@__fixed_roster_id, @__fixed_roster_scope},
                                                        
            };
    }

}