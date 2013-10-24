using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_question_that_has_substitutions_references_with_deeper_propagation_level : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperPropagationLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var autoPropagatedGroup = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new AutoPropagateQuestion
            {
                PublicKey = Guid.NewGuid(),
                Triggers = new List<Guid> { autoPropagatedGroup }
            });

            var autopropagatedGroup = new Group() { PublicKey = autoPropagatedGroup, Propagated = Propagate.AutoPropagated };

            autopropagatedGroup.Children.Add(new NumericQuestion()
            {
                PublicKey = underDeeperPropagationLevelQuestionId,
                StataExportCaption = underDeeperPropagationLevelQuestionVariableName
            });
            questionnaire.Children.Add(autopropagatedGroup);
            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithSubstitutionsId,
                QuestionText = string.Format("hello %{0}%", underDeeperPropagationLevelQuestionVariableName)
            });

            verifier = CreateQuestionnaireVerifier();
        };

        private Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        private It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        private It should_return_error_with_code__WB0019 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0019");

        private It should_return_error_with_two_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        private It should_return_first_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        private It should_return_first_error_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithSubstitutionsId);

        private It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        private It should_return_last_error_reference_with_id_of_underDeeperPropagationLevelQuestionVariableName = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(underDeeperPropagationLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperPropagationLevelQuestionId;
        private const string underDeeperPropagationLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
