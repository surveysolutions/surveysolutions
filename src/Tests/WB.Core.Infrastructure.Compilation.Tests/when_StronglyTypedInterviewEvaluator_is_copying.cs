using Machine.Specifications;
using WB.Core.Infrastructure.BaseStructures;

//should be moved to common test project or deleted after dynamic generation

namespace WB.Core.Infrastructure.Compilation.Tests
{
    internal class when_StronglyTypedInterviewEvaluator_is_copying
    {
        private Establish context = () =>
        {
            stronglyTypedInterviewEvaluator = new StronglyTypedInterviewEvaluator();

            stronglyTypedInterviewEvaluator.UpdateIntAnswer(IdOf.persons_count, new decimal[] { }, persons_count);
            stronglyTypedInterviewEvaluator.AddRoster(IdOf.hhMember, new decimal[] {  }, 1, null);
            stronglyTypedInterviewEvaluator.UpdateIntAnswer(IdOf.age, new decimal[] { 1 }, age);
        };

        private Because of = () =>
            copyResult = stronglyTypedInterviewEvaluator.Copy() as StronglyTypedInterviewEvaluator;

        private It should_value_of_top_roster = () =>
            (copyResult.interviewScopes[
                StronglyTypedInterviewEvaluator.GetRosterStringKey(StronglyTypedInterviewEvaluator.GetRosterKey(
                    new[] { IdOf.questionnaire }, emptyRosterVector))] as QuestionnaireLevel).persons_count.ShouldEqual(persons_count);

        private It should_roster_question = () =>
            (copyResult.interviewScopes[
                StronglyTypedInterviewEvaluator.GetRosterStringKey(StronglyTypedInterviewEvaluator.GetRosterKey(
                    IdOf.rostersIdToScopeMap[IdOf.hhMember] , new decimal[] { 1 }))] as HhMember).age.ShouldEqual(age);


        private static IInterviewExpressionState stronglyTypedInterviewEvaluator;
        private static StronglyTypedInterviewEvaluator copyResult;

        private static int persons_count = 1;
        private static int age = 11;

        private static decimal[] emptyRosterVector = new decimal[] { };

        private static string filePath;
    }
}
