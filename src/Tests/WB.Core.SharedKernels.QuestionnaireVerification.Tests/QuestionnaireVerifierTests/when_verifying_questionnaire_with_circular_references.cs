using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    [Ignore("C#")]
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
                            ConditionExpression = "[b]>0",
                            StataExportCaption = "a"
                        },
                        new TextQuestion
                        {
                            PublicKey = question2Id,
                            ConditionExpression = "[a]>0",
                            StataExportCaption = "b"
                        }
                    }
                }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor
                .Setup(x => x.IsSyntaxValid(Moq.It.IsAny<string>()))
                .Returns(true);

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression("[a]>0"))
                .Returns(new[] { question1Id.ToString() });

            expressionProcessor
                .Setup(x => x.GetIdentifiersUsedInExpression("[b]>0"))
                .Returns(new[] { question2Id.ToString()});

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_error_with_code__WB0056 = () =>
            resultErrors.First().Code.ShouldEqual("WB0056");

        It should_return_error_with_two_references = () =>
            resultErrors.First().References.Count().ShouldEqual(2);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
    }
}