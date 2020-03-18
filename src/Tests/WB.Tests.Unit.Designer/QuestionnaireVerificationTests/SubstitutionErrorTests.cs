using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    public class SubstitutionErrorTests
    {
        [Test]
        public void should_allow_using_self_in_title()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                Create.Question(questionId: Id.g1, title: "test %self%", variable: "variable1")
            );

            questionnaire.ExpectNoError("WB0017");
        }


        [Test]
        public void should_not_produce_substitution_error_for_custom_title_rosters()
        {
            var questionniare = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(title: "roster %rostertitle%",
                    customRosterTitle: true
                ));

            questionniare.ExpectNoError("WB0059");
        }
    }
}
