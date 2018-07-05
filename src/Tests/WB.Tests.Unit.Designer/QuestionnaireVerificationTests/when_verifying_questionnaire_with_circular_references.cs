using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_circular_references : QuestionnaireVerifierTestsContext
    {
            [NUnit.Framework.OneTimeSetUp] public void context () {
            var groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question1Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var question2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = groupId,
                    IsRoster = false,
                    Children = new List<IComposite>
                    {
                        new TextQuestion
                        {
                            PublicKey = question1Id,
                            ConditionExpression = "b>0",
                            StataExportCaption = "a"
                        },
                        new TextQuestion
                        {
                            PublicKey = question2Id,
                            ConditionExpression = "a>0",
                            StataExportCaption = "b"
                        }
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0056 () =>
            verificationMessages.ShouldContainError("WB0056");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.GetError("WB0056").MessageLevel.Should().Be(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_two_references () =>
            verificationMessages.GetError("WB0056").References.Count().Should().Be(2);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}