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
    internal class when_verifying_questionnaire_that_has_question_that_references_itself_in_condition : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
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
                            PublicKey = questionId,
                            ConditionExpression = "s546i==1",
                            StataExportCaption = "s546i"
                        }
                    }.ToReadOnlyCollection()
                }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression("s546i==1"))
                .Returns(new[] { "s546i" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_message_with_code__WB0056 = () =>
            verificationMessages.ShouldContainError("WB0056");

        It should_return_message_with_one_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_message_with_one_references_with_question_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_with_one_references_with_id_equals_questionId = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}