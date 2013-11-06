using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_1_orphan_auto_propagated_group_and_question_referencing_himself_inside :
        QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {

            autoPropagatedGroupId = Guid.Parse("13333333333333333333333333333333");
            questionWithSelfValidation = Guid.Parse("13333333333333333333333333333333");
            questionWithSelfValidationVariableName = "var";
            questionnaire = CreateQuestionnaireDocument();
            var chapter = new Group("chapter");
            var autopropagatedGroup = new Group() { PublicKey = autoPropagatedGroupId, Propagated = Propagate.AutoPropagated };

            autopropagatedGroup.Children.Add(new NumericQuestion()
            {
                PublicKey = questionWithSelfValidation,
                StataExportCaption = questionWithSelfValidationVariableName,
                ValidationExpression = "i'am validation expr"
            });

            chapter.Children.Add(autopropagatedGroup);
            questionnaire.Children.Add(chapter);

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.IsSyntaxValid(Moq.It.IsAny<string>())).Returns(true);

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new string[] { questionWithSelfValidationVariableName });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        private Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        private It should_return_1_errors = () =>
            resultErrors.Count().ShouldEqual(1);

        private It should_return_first_error_with_code__WB0009 = () =>
            resultErrors.First().Code.ShouldEqual("WB0009");

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSelfValidation;
        private static string questionWithSelfValidationVariableName;
        private static Guid autoPropagatedGroupId;
    }
}
