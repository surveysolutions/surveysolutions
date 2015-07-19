using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class when_generating_assembly_as_string_with_deep_stack : InterviewCompilerTestsContext
    {

        Establish context = () =>
        {
            compiler = CreateRoslynCompiler();
            referencedPortableAssemblies = CreateReferencesForCompiler();

            var classes = new Dictionary<string, string>();
            classes.Add("main", testClassToCompile);
            classes.Add(fileName, testClassToCompilePartTwo);

            generatedClasses = classes;
        };

        Because of = () => emitResult = IncreaseCallStackEndExec_TODO_Check_does_method_name_affect_stack(1);


        private static EmitResult IncreaseCallStackEndExec_TODO_Check_does_method_name_affect_stack(int a)
        {
            return a > staskDepthToAdd ?
                compiler.TryGenerateAssemblyAsStringAndEmitResult(id, generatedClasses, referencedPortableAssemblies, out resultAssembly) :
                IncreaseCallStackEndExec_TODO_Check_does_method_name_affect_stack(a + 1);
        }

        It should_succeded = () =>
            emitResult.Success.ShouldEqual(true);
        
        private static IDynamicCompiler compiler;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static string resultAssembly;
        private static EmitResult emitResult;
        private static Dictionary<string, string> generatedClasses;
        private static PortableExecutableReference[] referencedPortableAssemblies;

        private static string fileName = "validation:11111111111111111111111111111112";

        private static int staskDepthToAdd = 130;



        public static string testClassToCompile =
            @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection.Generated
{
    public class InterviewExpressionState_9a3ff0299518414ba8cfb720bfe1ff17 : AbstractInterviewExpressionState, IInterviewExpressionStateV2 
    {
        public InterviewExpressionState_9a3ff0299518414ba8cfb720bfe1ff17() 
        {
            var questionnaireLevelScope = new[] { IdOf.@__questionnaire };
            var questionnaireIdentityKey = Util.GetRosterKey(questionnaireLevelScope, Util.EmptyRosterVector);
            var questionnaireLevel = new QuestionnaireTopLevel(Util.EmptyRosterVector, questionnaireIdentityKey, this.GetRosterInstances, IdOf.conditionalDependencies, IdOf.structuralDependencies);
            this.InterviewScopes.Add(Util.GetRosterStringKey(questionnaireIdentityKey), questionnaireLevel);
        }

        private InterviewExpressionState_9a3ff0299518414ba8cfb720bfe1ff17(Dictionary<string, IExpressionExecutable> interviewScopes, Dictionary<string, List<string>> siblingRosters)
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

            targetLevel.UpdateGeoLocationAnswer(questionId, latitude,  longitude,  accuracy, altitude);
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
            return new InterviewExpressionState_9a3ff0299518414ba8cfb720bfe1ff17(this.InterviewScopes, this.SiblingRosters);
        }

        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return Clone() as IInterviewExpressionStateV2;
        }

        public void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
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

    }


        //generate QuestionnaireLevel
        
    internal partial class QuestionnaireTopLevel : AbstractConditionalLevel<QuestionnaireTopLevel>, IExpressionExecutable
    {
        public QuestionnaireTopLevel(decimal[] rosterVector, Identity[] rosterKey, Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances, 
                Dictionary<Guid, Guid[]> conditionalDependencies, Dictionary<Guid, Guid[]> structureDependencies)
            : base(rosterVector, rosterKey, getInstances, conditionalDependencies, structureDependencies)
        {
        
                    EnablementStates.Add(@__2a0c684de41546d99f8044062f9c5679_state.ItemId, @__2a0c684de41546d99f8044062f9c5679_state);
                        
                    EnablementStates.Add(@__a_state.ItemId, @__a_state);
            
            ValidationExpressions.Add(new Identity(IdOf.@__a_id, rosterVector), new Func<bool>[] {                             
                                  
                    () => this.IsAnswerEmpty(a) || IsValid_a() , 
                            }); 
                    AddUpdaterToMap(IdOf.@__a_id, (double? val) => {@__a  = val; });
                
        
        }                                 

        public IExpressionExecutable CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutable>> getInstances)
        {
            var level = new QuestionnaireTopLevel(this.RosterVector, this.RosterKey, getInstances, ConditionalDependencies, StructuralDependencies)
            {
                ValidAnsweredQuestions = new HashSet<Guid>(this.ValidAnsweredQuestions),
                InvalidAnsweredQuestions = new HashSet<Guid>(this.InvalidAnsweredQuestions),
                        
                @__a = this.@__a,        
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

        private double? @__a = null;
        private ConditionalState @__a_state = new ConditionalState(IdOf.@__a_id);
        public double? a
        {
            get { return @__a_state.State != State.Disabled ? this.@__a : null; }
            
        }
                
        
                        
                // groups condition states
                private ConditionalState @__2a0c684de41546d99f8044062f9c5679_state = new ConditionalState(IdOf.@__2a0c684de41546d99f8044062f9c5679_id, ItemType.Group);
                        
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
        
        public static class IdOf
        {
            public static readonly Guid @__questionnaire = Guid.Parse(""4f66d228-f2f5-4e32-98f4-2de7079fea6b""); 
            //questions
                        public static readonly Guid @__a_id = Guid.Parse(""bae70e5b-e198-a698-867e-16312511c8a1"");
                        //groups
                        public static readonly Guid @__2a0c684de41546d99f8044062f9c5679_id = Guid.Parse(""2a0c684d-e415-46d9-9f80-44062f9c5679"");
                        //rosters
                        
            public static readonly Guid[] @__questionnaire_scope = new[] {@__questionnaire};

                        
            public static Dictionary<Guid, Guid[]> conditionalDependencies = new Dictionary<Guid, Guid[]>()
            {            
                            };

            public static Dictionary<Guid, Guid[]> structuralDependencies = new Dictionary<Guid, Guid[]>()
            {
                                { Guid.Parse(""2a0c684d-e415-46d9-9f80-44062f9c5679""), new Guid[]{
                                    Guid.Parse(""bae70e5b-e198-a698-867e-16312511c8a1""),
                                }},
                            };

            public static Dictionary<Guid, Guid[]> parentScopeMap = new Dictionary<Guid, Guid[]>
            {
                //questions
                                {@__a_id, @__questionnaire_scope},
                                //groups
                                {@__2a0c684de41546d99f8044062f9c5679_id, @__questionnaire_scope},
                                //rosters
                                                        
            };
        }    
}";

        public static string testClassToCompilePartTwo =
            @"using System;
              using System.Collections.Generic;
              using System.Linq;
              using System.Text.RegularExpressions;

              namespace WB.Core.SharedKernels.DataCollection.Generated
{
   internal partial class QuestionnaireTopLevel
   {   
      private bool IsValid_a()
      {
          return (a == 100) || ((a == 200) || ((a == 300) || ((a == 400) || ((a == 501) || ((a == 502) || ((a == 600) || ((a == 701) || ((a == 702) || ((a == 801) || ((a == 802) || ((a == 901) || ((a == 902) || ((a == 903) || ((a == 1001) || ((a == 1002) || ((a == 1101) || ((a == 1102) || ((a == 1201) || ((a == 1202) || ((a == 1301) || ((a == 1302) || ((a == 1401) || ((a == 1402) || ((a == 1403) || ((a == 1404) || ((a == 1405) || ((a == 1406) || ((a == 1407) || ((a == 1408) || ((a == 1409) || ((a == 1410) || ((a == 1411) || ((a == 1412) || ((a == 1413) || ((a == 1500) || ((a == 1601) || ((a == 1602) || ((a == 1701) || ((a == 1702) || ((a == 1703) || ((a == 1800) || ((a == 1901) || ((a == 1902) || ((a == 1903) || ((a == 1904) || ((a == 2000) || ((a == 2101) || ((a == 2102) || ((a == 2103) || ((a == 2104) || ((a == 2105) || ((a == 2106) || ((a == 2107) || ((a == 2201) || ((a == 2202) || ((a == 2203) || ((a == 2301) || ((a == 2302) || ((a == 2303) || ((a == 2304) || ((a == 2305) || ((a == 2401) || ((a == 2402) || ((a == 2403) || ((a == 2404) || ((a == 2501) || ((a == 2502) || ((a == 2503) || ((a == 2504) || ((a == 2505) || ((a == 2601) || ((a == 2602) || ((a == 2603) || ((a == 2604) || ((a == 2605) || ((a == 2606) || ((a == 2607) || ((a == 2608) || ((a == 2701) || ((a == 2702) || ((a == 2703) || ((a == 2704) || ((a == 2705) || ((a == 2706) || ((a == 2801) || ((a == 2802) || ((a == 2803) || ((a == 2804) || ((a == 2805) || ((a == 2806) || ((a == 2807) || ((a == 2808) || ((a == 2809) || ((a == 2810) || ((a == 2901) || ((a == 2902) || ((a == 3001) || ((a == 3002) || ((a == 3101) || ((a == 3102) || ((a == 3103) || ((a == 3104) || ((a == 3105) || ((a == 3201) || ((a == 3202) || ((a == 3203) || ((a == 3301) || ((a == 3302) || ((a == 3401) || ((a == 3402) || ((a == 3403) || ((a == 3404) || ((a == 3405) || ((a == 3406) || ((a == 3407) || ((a == 3408) || ((a == 3409) || ((a == 3410) || ((a == 3501) || ((a == 3502) || ((a == 3503) || ((a == 3504) || ((a == 3505) || ((a == 3506) || ((a == 3507) || ((a == 3508) || ((a == 3509) || ((a == 3601) || ((a == 3602) || ((a == 3701) || ((a == 3702) || ((a == 3703) || ((a == 3704) || ((a == 3705) || ((a == 3706) || ((a == 3801) || ((a == 3802) || ((a == 3803) || ((a == 3804) || ((a == 3805) || ((a == 3901) || ((a == 3902) || ((a == 3903) || ((a == 3904) || ((a == 3905) || ((a == 4001) || ((a == 4002) || ((a == 4003) || ((a == 4004) || ((a == 4005) || ((a == 4006) || ((a == 4007) || ((a == 4100) || ((a == 4201) || ((a == 4202) || ((a == 4203) || ((a == 4204) || ((a == 4301) || ((a == 4302) || ((a == 4304) || ((a == 4401) || ((a == 4402) || ((a == 4403) || ((a == 4404) || ((a == 4501) || ((a == 4502) || ((a == 4503) || ((a == 4504) || ((a == 4600) || ((a == 4701) || ((a == 4702) || ((a == 4801) || ((a == 4802) || ((a == 4803) || ((a == 4804) || ((a == 4805) || ((a == 4806) || ((a == 4807) || ((a == 4808) || ((a == 4809) || ((a == 4811) || ((a == 4901) || ((a == 4902) || ((a == 4903) || ((a == 4904) || ((a == 4905) || ((a == 4906) || ((a == 4907) || ((a == 4908) || ((a == 4909) || ((a == 4910) || ((a == 4911) || ((a == 4912) || ((a == 4913) || ((a == 4914) || ((a == 4915) || ((a == 4916) || ((a == 4917) || ((a == 4918) || ((a == 4919) || ((a == 4920) || ((a == 4921) || ((a == 4922) || ((a == 4923) || ((a == 5001) || ((a == 5002) || ((a == 5003) || ((a == 5004) || ((a == 5005) || ((a == 5006) || ((a == 5100) || ((a == 5200) || ((a == 5301) || ((a == 5302) || ((a == 5400) || ((a == 5500) || ((a == 5600) || ((a == 5700) || ((a == 5801) || ((a == 5802) || ((a == 5901) || ((a == 5902) || ((a == 6001) || ((a == 6002) || ((a == 6101) || ((a == 6102) || ((a == 6201) || ((a == 6202) || ((a == 6203) || ((a == 6204) || ((a == 6205) || ((a == 6301) || ((a == 6302) || ((a == 6401) || ((a == 6402) || ((a == 6501) || ((a == 6502) || ((a == 6503) || ((a == 6504) || ((a == 6601) || ((a == 6602) || ((a == 6701) || ((a == 6702) || ((a == 6703) || ((a == 6704) || ((a == 6801) || ((a == 6802) || ((a == 6901) || ((a == 6902) || ((a == 6903) || ((a == 6904) || ((a == 7001) || ((a == 7002) || ((a == 7101) || ((a == 7102) || ((a == 7103) || ((a == 7200) || ((a == 7301) || ((a == 7302) || ((a == 7400) || ((a == 7501) || ((a == 7502) || ((a == 7503) || ((a == 7600) || ((a == 7700) || ((a == 7800) || ((a == 7900) || ((a == 8001) || ((a == 8002) || ((a == 8101) || ((a == 8102) || ((a == 8103) || ((a == 8200) || ((a == 8300) || ((a == 8400) || ((a == 8501) || ((a == 8502) || ((a == 8601) || ((a == 8602) || ((a == 8700) || ((a == 8801) || ((a == 8802) || ((a == 8901) || ((a == 8902) || ((a == 8903) || ((a == 9001) || ((a == 9002) || ((a == 9003) || ((a == 9004) || ((a == 9005) || ((a == 9101) || ((a == 9102) || ((a == 9200) || ((a == 9300) || ((a == 9401) || ((a == 9402) || ((a == 9403) || ((a == 9500) || ((a == 9601) || ((a == 9602) || ((a == 9701) || ((a == 9702) || ((a == 9801) || ((a == 9802) || ((a == 9900) || ((a == 10000) || ((a == 10100) || ((a == 10201) || ((a == 10202) || ((a == 10301) || ((a == 10302) || ((a == 10401) || ((a == 10402) || ((a == 10403) || ((a == 10501) || ((a == 10502) || ((a == 10601) || ((a == 10602) || ((a == 10701) || ((a == 10702) || ((a == 10703) || ((a == 10704) || ((a == 10705) || ((a == 10706) || ((a == 10801) || ((a == 10802) || ((a == 10803) || ((a == 10804) || ((a == 10805) || ((a == 10806) || ((a == 10807) || ((a == 10808) || ((a == 10809) || ((a == 10900) || ((a == 11000) || ((a == 11100) || ((a == 11201) || ((a == 11202) || ((a == 11203) || ((a == 11204) || ((a == 11205) || ((a == 11206) || ((a == 11207) || ((a == 11208) || ((a == 11209) || ((a == 11210) || ((a == 11211) || ((a == 11212) || ((a == 11213) || ((a == 11214) || ((a == 11301) || ((a == 11302) || ((a == 11303) || ((a == 11304) || ((a == 11305) || ((a == 11306) || ((a == 11307) || ((a == 11308) || ((a == 11309) || ((a == 11401) || ((a == 11402) || ((a == 11403) || ((a == 11404) || ((a == 11501) || ((a == 11502) || ((a == 11503) || ((a == 11504) || ((a == 11505) || ((a == 11601) || ((a == 11602) || ((a == 11603) || ((a == 11604) || ((a == 11605) || ((a == 11606) || ((a == 11701) || ((a == 11702) || ((a == 11800) || ((a == 11901) || ((a == 11902) || ((a == 11903) || ((a == 11904) || ((a == 11905) || ((a == 12001) || ((a == 12002) || ((a == 12003) || ((a == 12004) || ((a == 12101) || ((a == 12102) || ((a == 12103) || ((a == 12104) || ((a == 12105) || ((a == 12106) || ((a == 12107) || ((a == 12108) || ((a == 12109) || ((a == 12110) || ((a == 12111) || ((a == 12112) || ((a == 12113) || ((a == 12114) || ((a == 12115) || ((a == 12116) || ((a == 12201) || ((a == 12202) || ((a == 12203) || ((a == 12204) || ((a == 12205) || ((a == 12301) || ((a == 12302) || ((a == 12401) || ((a == 12402) || ((a == 12403) || ((a == 12404) || ((a == 12405) || ((a == 12406) || ((a == 12501) || ((a == 12502) || ((a == 12601) || ((a == 12602) || ((a == 12603) || ((a == 12700) || ((a == 12800) || ((a == 12900) || ((a == 13001) || ((a == 13002) || ((a == 13003) || ((a == 13004) || ((a == 13005) || ((a == 13101) || ((a == 13102) || ((a == 13103) || ((a == 13201) || ((a == 13202) || ((a == 13203) || ((a == 13301) || ((a == 13302) || ((a == 13303) || ((a == 13304) || ((a == 13401) || ((a == 13402) || ((a == 13403) || ((a == 13404) || ((a == 13405) || ((a == 13501) || ((a == 13502) || ((a == 13600) || ((a == 13701) || ((a == 13702) || ((a == 13703) || ((a == 13800) || ((a == 13901) || ((a == 13902) || ((a == 13903) || ((a == 14001) || ((a == 14002) || ((a == 14100) || ((a == 14200) || ((a == 14301) || ((a == 14302) || ((a == 14400) || ((a == 14501) || ((a == 14502) || ((a == 14601) || ((a == 14602) || ((a == 14603) || ((a == 14604) || ((a == 14605) || ((a == 14606) || ((a == 14607) || ((a == 14701) || ((a == 14702) || ((a == 14703) || ((a == 14704) || ((a == 14705) || ((a == 14706) || ((a == 14707) || ((a == 14708) || ((a == 14709) || ((a == 14710) || ((a == 14711) || ((a == 14712) || ((a == 14713) || ((a == 14714) || ((a == 14715) || ((a == 14716) || ((a == 14717) || ((a == 14718) || ((a == 14719) || ((a == 14720))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))));
      }      
   }
}
";
    }
}
