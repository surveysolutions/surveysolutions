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

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_multimedia_question_which_used_as_link_source : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                Create.FixedRoster(variable: "varRoster",
                    fixedTitles: new[] {"1", "2"},
                    children: new IComposite[]
                    {
                        new MultimediaQuestion()
                        {
                            PublicKey = multimediaQuestionId,
                            StataExportCaption = "var1"
                        }
                    }),
                new SingleQuestion()
                {
                    PublicKey = questionWhichUsesMultimediaAsLinkSource,
                    LinkedToQuestionId = multimediaQuestionId,
                    StataExportCaption = "var2"
                });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0012 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0012");

        It should_return_message_with_2_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_first_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_reference_with_id_of_questionWhichUsesMultimediaAsLinkSource = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionWhichUsesMultimediaAsLinkSource);

        It should_return_second_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_reference_with_id_of_multimediaQuestionId = () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(multimediaQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multimediaQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid questionWhichUsesMultimediaAsLinkSource = Guid.Parse("20000000000000000000000000000000");
    }
}
