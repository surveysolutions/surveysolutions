using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class RosterVerificationTests : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void when_exists_roster_with_roster_title_question_then_cyrcular_referance_should_not_be_exists()
        {
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(intQuestionId, variable: "i1"),
                Create.NumericRoster(rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId:rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                {
                    Create.TextQuestion(rosterTitleQuestionId, variable: "tb")
                })
            });

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            verificationMessages.ShouldNotContainError("WB0056");
            verificationMessages.GetError("WB0056").Should().BeNull();
        }

        [Test]
        public void should_validate_location_of_roster_title()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.FixedRoster(title: "Roster %rostertitle%",
                    rosterId: Id.gA,
                    children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(variable: "test1", id: Id.g1)
                    }
                ))
                .ExpectError("WB0059");
        }

        [Test]
        public void should_validate_location_of_roster_title_for_numeric_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(id: Id.g1),
                Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, title: "title %rostertitle%")
            )
            .ExpectError("WB0059");
        }
    }
}
