using System;
using Machine.Specifications;

//should be moved to common test project or deleted after dynamic generation
using WB.Core.SharedKernels.ExpressionProcessing;

namespace WB.Core.Infrastructure.Compilation.Tests
{
    internal class when_StronglyTypedInterviewEvaluator_with_one_roster_is_cloning
    {
        Establish context = () =>
        {
            stronglyTypedInterviewEvaluator = new StronglyTypedInterviewEvaluator();

            stronglyTypedInterviewEvaluator.UpdateIntAnswer(StronglyTypedInterviewEvaluator.IdOf.persons_count, emptyRosterVector, persons_count);
            stronglyTypedInterviewEvaluator.AddRoster(StronglyTypedInterviewEvaluator.IdOf.hhMember, emptyRosterVector, 1, null);
            stronglyTypedInterviewEvaluator.UpdateIntAnswer(StronglyTypedInterviewEvaluator.IdOf.age, firstLevelRosterVector, age);
        };

        Because of = () =>
            copyResult = stronglyTypedInterviewEvaluator.Clone() as StronglyTypedInterviewEvaluator;

        It should_clone_value_of_top_roster = () =>
            (copyResult.InterviewScopes[GetRosterKey(new[] { StronglyTypedInterviewEvaluator.IdOf.questionnaire }, emptyRosterVector)] as StronglyTypedInterviewEvaluator.QuestionnaireLevel)
            .persons_count.ShouldEqual(persons_count);

        It should_clone_answer_of_roster_question = () =>
            (copyResult.InterviewScopes[GetRosterKey(StronglyTypedInterviewEvaluator.IdOf.rostersIdToScopeMap[StronglyTypedInterviewEvaluator.IdOf.hhMember], firstLevelRosterVector)] as StronglyTypedInterviewEvaluator.HhMember_type)
            .age.ShouldEqual(age);


        private static string GetRosterKey(Guid[] rosterScopeIds, decimal[] rosterVector)
        {
            return Util.GetRosterStringKey(
                Util.GetRosterKey(rosterScopeIds, rosterVector));
        }
        private static IInterviewExpressionState stronglyTypedInterviewEvaluator;
        private static StronglyTypedInterviewEvaluator copyResult;

        private static int persons_count = 1;
        private static int age = 11;

        private static string filePath;
        private static readonly decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] firstLevelRosterVector = new [] { 1.0m };
    }
}
