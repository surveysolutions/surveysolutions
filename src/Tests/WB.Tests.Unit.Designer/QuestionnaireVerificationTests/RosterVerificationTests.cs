using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests;

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
            verificationMessages.GetError("WB0056").ShouldBeNull();
        }
    }
}