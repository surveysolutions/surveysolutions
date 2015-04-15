using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_single_option_linked_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new Group()
            {
                PublicKey = Guid.NewGuid(),
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                RosterFixedTitles = new[] { "1", "2" },
                VariableName = "varRoster",
                Children = new List<IComposite>
                {
                    new TextQuestion()
                    {
                        PublicKey = sourceLinkedQuestionId,
                        QuestionType = QuestionType.Text,
                        StataExportCaption = "var1"
                    }
                },
            }, new SingleQuestion()
            {
                PublicKey = linkedQuestionId,
                StataExportCaption = "var2",
                Featured = true,
                QuestionType = QuestionType.SingleOption,
                LinkedToQuestionId = sourceLinkedQuestionId
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0090 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0090");

        It should_return_error_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_error_reference_with_id_of_questionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid sourceLinkedQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}
