
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace WB.Core.SharedKernels.DataCollection.Generated
{
    public class InterviewExpressionState_7d59f35ebecf45cb9a890d03c56759bc : AbstractInterviewExpressionState
    {
        public InterviewExpressionState_7d59f35ebecf45cb9a890d03c56759bc()
        {
            var questionnaireLevelScope = new[] { IdOf.@__questionnaire };
            var questionnaireIdentityKey = Util.GetRosterKey(questionnaireLevelScope, Util.EmptyRosterVector);
            var questionnaireLevel = new QuestionnaireTopLevel(Util.EmptyRosterVector, questionnaireIdentityKey, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
            this.InterviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
        }

        private InterviewExpressionState_7d59f35ebecf45cb9a890d03c56759bc(Dictionary<string, IExpressionExecutable> interviewScopes, Dictionary<string, List<string>> siblingRosters)
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
            return new InterviewExpressionState_7d59f35ebecf45cb9a890d03c56759bc(this.InterviewScopes, this.SiblingRosters);
        }

    }


    //generate QuestionnaireLevel

    internal partial class QuestionnaireTopLevel : AbstractConditionalLevel<QuestionnaireTopLevel>, IExpressionExecutable
    {
        public QuestionnaireTopLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {

            EnablementStates.Add(@__bac5932193574680a2f613cc86a095bc_state.ItemId, @__bac5932193574680a2f613cc86a095bc_state);

            EnablementStates.Add(@__num_state.ItemId, @__num_state);
            AddUpdaterToMap(IdOf.@__num_id, (long? val) => { @__num = val; });
            EnablementStates.Add(@__fin_state.ItemId, @__fin_state);
            AddUpdaterToMap(IdOf.@__fin_id, (string val) => { @__fin = val; });

            RosterGenerators.Add(IdOf.@__fam_id, (decimals, identities) => new @__d146baaba08340c38bedbb7ff3d76eed(decimals, identities, this, this.GetInstances, this.ConditionalDependencies, this.StructuralDependencies));

            _conditionExpressions.Add(Verifier(IsEnabled_fin, @__fin_state.ItemId, @__fin_state));

        }

        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            var level = new QuestionnaireTopLevel(this.RosterVector, this.RosterKey, getInstances, ConditionalDependencies, StructuralDependencies)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),

                @__num = this.@__num,

                @__fin = this.@__fin,
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


        private long? @__num = null;
        private ConditionalState @__num_state = new ConditionalState(IdOf.@__num_id);
        public long? num
        {
            get { return @__num_state.State != State.Disabled ? this.@__num : null; }

        }



        private string @__fin = null;
        private ConditionalState @__fin_state = new ConditionalState(IdOf.@__fin_id);
        public string fin
        {
            get { return @__fin_state.State != State.Disabled ? this.@__fin : null; }

        }
        private bool IsEnabled_fin()
        {
            var sum = fam.Sum(y => y.frnd.Sum(z => z.pet_age));
            return sum > 10;
        }


        public IEnumerable<@__d146baaba08340c38bedbb7ff3d76eed> fam
        {
            get
            {
                var rosters = this.GetInstances(new Identity[0], IdOf.@__fam_scope.Last());
                return rosters == null ? new List<@__d146baaba08340c38bedbb7ff3d76eed>() : rosters.Select(x => x as @__d146baaba08340c38bedbb7ff3d76eed).ToList();
            }
        }
        // groups condition states
        private ConditionalState @__bac5932193574680a2f613cc86a095bc_state = new ConditionalState(IdOf.@__bac5932193574680a2f613cc86a095bc_id, ItemType.Group);

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
    internal partial class @__d146baaba08340c38bedbb7ff3d76eed : AbstractConditionalLevel<@__d146baaba08340c38bedbb7ff3d76eed>, IExpressionExecutable
    {
        public @__d146baaba08340c38bedbb7ff3d76eed(decimal[] rosterVector, Identity[] rosterKey, QuestionnaireTopLevel parent, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {
            this.@_parent = parent;
        }

        private @__d146baaba08340c38bedbb7ff3d76eed(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {

            EnablementStates.Add(@__fam_state.ItemId, @__fam_state);


            EnablementStates.Add(@__title_state.ItemId, @__title_state);
            AddUpdaterToMap(IdOf.@__title_id, (string val) => { @__title = val; });

            EnablementStates.Add(@__P_age_state.ItemId, @__P_age_state);
            AddUpdaterToMap(IdOf.@__P_age_id, (long? val) => { @__P_age = val; });

            EnablementStates.Add(@__pet_state.ItemId, @__pet_state);
            AddUpdaterToMap(IdOf.@__pet_id, (long? val) => { @__pet = val; });

            RosterGenerators.Add(IdOf.@__frnd_id, (decimals, identities) => new @__334534567c354f67b76b428de1a5d6e0(decimals, identities, this, this.GetInstances, this.ConditionalDependencies, this.StructuralDependencies));


        }

        private QuestionnaireTopLevel @_parent;

        public long? num { get { return this.@_parent.num; } }
        public string fin { get { return this.@_parent.fin; } }

        public IEnumerable<@__d146baaba08340c38bedbb7ff3d76eed> fam { get { return this.@_parent.fam; } }

        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            var level = new @__d146baaba08340c38bedbb7ff3d76eed(this.RosterVector, this.RosterKey, getInstances, ConditionalDependencies, StructuralDependencies)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),


                @__title = this.@__title,

                @__P_age = this.@__P_age,

                @__pet = this.@__pet,
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



        private string @__title = null;
        private ConditionalState @__title_state = new ConditionalState(IdOf.@__title_id);
        public string title
        {
            get { return @__title_state.State != State.Disabled ? this.@__title : null; }

        }


        private long? @__P_age = null;
        private ConditionalState @__P_age_state = new ConditionalState(IdOf.@__P_age_id);
        public long? P_age
        {
            get { return @__P_age_state.State != State.Disabled ? this.@__P_age : null; }

        }


        private long? @__pet = null;
        private ConditionalState @__pet_state = new ConditionalState(IdOf.@__pet_id);
        public long? pet
        {
            get { return @__pet_state.State != State.Disabled ? this.@__pet : null; }

        }

        //groups
        //rosters
        public IEnumerable<@__334534567c354f67b76b428de1a5d6e0> frnd
        {
            get
            {
                IEnumerable<IExpressionExecutable> rosters = this.GetInstances(this.RosterKey, IdOf.@__frnd_scope.Last());
                return rosters == null ? new List<@__334534567c354f67b76b428de1a5d6e0>() : rosters.Select(x => x as @__334534567c354f67b76b428de1a5d6e0).ToList();
            }
        }



        private ConditionalState @__fam_state = new ConditionalState(IdOf.@__fam_id, ItemType.Group);




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
    internal partial class @__334534567c354f67b76b428de1a5d6e0 : AbstractConditionalLevel<@__334534567c354f67b76b428de1a5d6e0>, IExpressionExecutable
    {
        public @__334534567c354f67b76b428de1a5d6e0(decimal[] rosterVector, Identity[] rosterKey, @__d146baaba08340c38bedbb7ff3d76eed parent, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : this(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {
            this.@_parent = parent;
        }

        private @__334534567c354f67b76b428de1a5d6e0(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances,
            Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {

            EnablementStates.Add(@__frnd_state.ItemId, @__frnd_state);


            EnablementStates.Add(@__petsName_state.ItemId, @__petsName_state);
            AddUpdaterToMap(IdOf.@__petsName_id, (string val) => { @__petsName = val; });

            EnablementStates.Add(@__pet_age_state.ItemId, @__pet_age_state);
            AddUpdaterToMap(IdOf.@__pet_age_id, (long? val) => { @__pet_age = val; });


        }

        private @__d146baaba08340c38bedbb7ff3d76eed @_parent;

        public string title { get { return this.@_parent.title; } }
        public long? P_age { get { return this.@_parent.P_age; } }
        public long? pet { get { return this.@_parent.pet; } }
        public long? num { get { return this.@_parent.num; } }
        public string fin { get { return this.@_parent.fin; } }

        public IEnumerable<@__334534567c354f67b76b428de1a5d6e0> frnd { get { return this.@_parent.frnd; } }
        public IEnumerable<@__d146baaba08340c38bedbb7ff3d76eed> fam { get { return this.@_parent.fam; } }

        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            var level = new @__334534567c354f67b76b428de1a5d6e0(this.RosterVector, this.RosterKey, getInstances, ConditionalDependencies, StructuralDependencies)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),


                @__petsName = this.@__petsName,

                @__pet_age = this.@__pet_age,
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



        private string @__petsName = null;
        private ConditionalState @__petsName_state = new ConditionalState(IdOf.@__petsName_id);
        public string petsName
        {
            get { return @__petsName_state.State != State.Disabled ? this.@__petsName : null; }

        }


        private long? @__pet_age = null;
        private ConditionalState @__pet_age_state = new ConditionalState(IdOf.@__pet_age_id);
        public long? pet_age
        {
            get { return @__pet_age_state.State != State.Disabled ? this.@__pet_age : null; }

        }

        //groups
        //rosters



        private ConditionalState @__frnd_state = new ConditionalState(IdOf.@__frnd_id, ItemType.Group);




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
            this.@_parent = parentLevel as @__d146baaba08340c38bedbb7ff3d76eed;
        }

        public IExpressionExecutable GetParent()
        {
            return this.@_parent;
        }
    }

    public static class IdOf
    {
        public static readonly Guid @__questionnaire = Guid.Parse("aa8ea049-87ec-4559-8bf7-e76542f2dc5e");
        //questions
        public static readonly Guid @__num_id = Guid.Parse("3fdcd883-5ebf-4451-1f6c-7db452fa289f");
        public static readonly Guid @__fin_id = Guid.Parse("ddc77110-aea0-ded6-0527-86e7177d468e");
        public static readonly Guid @__title_id = Guid.Parse("5a78d0c4-c9e3-8789-39d7-4cb3a085f64c");
        public static readonly Guid @__P_age_id = Guid.Parse("2dceb9a8-fd6d-7510-11b9-26bd1d8e9f9b");
        public static readonly Guid @__pet_id = Guid.Parse("1b8fc07e-70cd-9a2a-9405-ac6fc08ba02e");
        public static readonly Guid @__petsName_id = Guid.Parse("636489ac-c56d-750b-0e55-c4d5cfa804fb");
        public static readonly Guid @__pet_age_id = Guid.Parse("9fe10ba6-a07a-a779-3097-78e97cd5816b");
        //groups
        public static readonly Guid @__bac5932193574680a2f613cc86a095bc_id = Guid.Parse("bac59321-9357-4680-a2f6-13cc86a095bc");
        //rosters
        public static readonly Guid @__fam_id = Guid.Parse("a39cf4a2-d54a-41fd-b99f-738fb3160c15");
        public static readonly Guid @__frnd_id = Guid.Parse("c463840d-d4ec-6d10-e41f-befbcfba856b");

        public static readonly Guid[] @__questionnaire_scope = new[] { @__questionnaire };

        public static readonly Guid[] @__fam_scope = new[] { Guid.Parse("3fdcd883-5ebf-4451-1f6c-7db452fa289f") };
        public static readonly Guid[] @__frnd_scope = new[] { Guid.Parse("3fdcd883-5ebf-4451-1f6c-7db452fa289f"), Guid.Parse("1b8fc07e-70cd-9a2a-9405-ac6fc08ba02e") };

        public static Dictionary<Guid, Guid[]> conditionalDependencies = new Dictionary<Guid, Guid[]>()
            {            
                                {Guid.Parse("ddc77110-aea0-ded6-0527-86e7177d468e"), new Guid[]{
                                    Guid.Parse("9fe10ba6-a07a-a779-3097-78e97cd5816b"),
                                    Guid.Parse("aa8ea049-87ec-4559-8bf7-e76542f2dc5e"),
                                    Guid.Parse("aa8ea049-87ec-4559-8bf7-e76542f2dc5e"),
                                }},
                            };

        public static Dictionary<Guid, Guid[]> structuralDependencies = new Dictionary<Guid, Guid[]>()
            {
                                { Guid.Parse("bac59321-9357-4680-a2f6-13cc86a095bc"), new Guid[]{
                                    Guid.Parse("3fdcd883-5ebf-4451-1f6c-7db452fa289f"),
                                    Guid.Parse("a39cf4a2-d54a-41fd-b99f-738fb3160c15"),
                                    Guid.Parse("ddc77110-aea0-ded6-0527-86e7177d468e"),
                                }},
                                { Guid.Parse("a39cf4a2-d54a-41fd-b99f-738fb3160c15"), new Guid[]{
                                    Guid.Parse("5a78d0c4-c9e3-8789-39d7-4cb3a085f64c"),
                                    Guid.Parse("2dceb9a8-fd6d-7510-11b9-26bd1d8e9f9b"),
                                    Guid.Parse("1b8fc07e-70cd-9a2a-9405-ac6fc08ba02e"),
                                    Guid.Parse("c463840d-d4ec-6d10-e41f-befbcfba856b"),
                                }},
                                { Guid.Parse("c463840d-d4ec-6d10-e41f-befbcfba856b"), new Guid[]{
                                    Guid.Parse("636489ac-c56d-750b-0e55-c4d5cfa804fb"),
                                    Guid.Parse("9fe10ba6-a07a-a779-3097-78e97cd5816b"),
                                }},
                            };

        public static Dictionary<Guid, Guid[]> parentScopeMap = new Dictionary<Guid, Guid[]>
            {
                //questions
                                {@__num_id, @__questionnaire_scope},
                                {@__fin_id, @__questionnaire_scope},
                                {@__title_id, @__fam_scope},
                                {@__P_age_id, @__fam_scope},
                                {@__pet_id, @__fam_scope},
                                {@__petsName_id, @__frnd_scope},
                                {@__pet_age_id, @__frnd_scope},
                                //groups
                                {@__bac5932193574680a2f613cc86a095bc_id, @__questionnaire_scope},
                                //rosters
                                {@__fam_id, @__fam_scope},
                                {@__frnd_id, @__frnd_scope},
                                                        
            };
    }

}