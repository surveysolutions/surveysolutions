using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_circular_references : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
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
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_message_with_code__WB0056 = () =>
            verificationMessages.ShouldContainError("WB0056");

        It should_return_message_with_level_general = () =>
            verificationMessages.GetError("WB0056").MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_two_references = () =>
            verificationMessages.GetError("WB0056").References.Count().ShouldEqual(2);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}